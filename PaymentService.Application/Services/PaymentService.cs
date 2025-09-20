using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Simulator;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;
using System.Text.Json;

namespace PaymentService.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IValidator<PaymentInitiateRequestDTO> _paymentInitiateValidator;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserPaymentMethodRepository _userPaymentMethodRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGatewayResponseRepository _gatewayResponseRepository;
        private readonly IPaymentProviderConfigurationRepository _configRepository;
        private readonly PaymentGatewaySimulator _paymentGatewaySimulator;
        private readonly PaymentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IValidator<PaymentInitiateRequestDTO> paymentInitiateValidator,
            IPaymentRepository paymentRepository,
            IUserPaymentMethodRepository userPaymentMethodRepository,
            ITransactionRepository transactionRepository,
            IGatewayResponseRepository gatewayResponseRepository,
            IPaymentProviderConfigurationRepository configRepository,
            PaymentDbContext dbContext,
            IConfiguration configuration)
        {
            _paymentInitiateValidator = paymentInitiateValidator;
            _paymentRepository = paymentRepository;
            _userPaymentMethodRepository = userPaymentMethodRepository;
            _transactionRepository = transactionRepository;
            _gatewayResponseRepository = gatewayResponseRepository;
            _configRepository = configRepository;
            _paymentGatewaySimulator = new PaymentGatewaySimulator();
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<PaymentInitiateResponseDTO> InitiatePaymentAsync(PaymentInitiateRequestDTO request)
        {
            ValidationResult vr = await _paymentInitiateValidator.ValidateAsync(request);
            if (!vr.IsValid)
                throw new ArgumentException($"Validation failed: {string.Join("; ", vr.Errors.Select(e => e.ErrorMessage))}");

            if (!request.PaymentMethodTypeId.HasValue)
                throw new ArgumentException("PaymentMethodTypeId is required.");

            var methodType = request.PaymentMethodTypeId.Value;

            // Create/resolve a UPM for non-COD; COD returns null.
            var userPaymentMethodId = await ResolveUserPaymentMethodIdOrCreateAsync(request.UserId, methodType);

            // Return existing in-progress (not Failed/Canceled)
            var existing = await _paymentRepository.GetPaymentByOrderAsync(request.OrderId, request.UserId);
            if (existing != null &&
                existing.PaymentStatusId != (int)PaymentStatusEnum.Failed &&
                existing.PaymentStatusId != (int)PaymentStatusEnum.Canceled)
            {
                return new PaymentInitiateResponseDTO
                {
                    PaymentId = existing.PaymentId,
                    PaymentUrl = existing.PaymentUrl ?? string.Empty,
                    Status = existing.PaymentStatus.Name.ToString()
                };
            }

            using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Create Payment (Pending)
                var payment = await _paymentRepository.InitiatePaymentAsync(
                    request.OrderId,
                    request.UserId,
                    request.Amount,
                    request.Currency,
                    userPaymentMethodId,
                    (int)methodType,
                    paymentUrl: null);

                // COD → no PG
                if (methodType == PaymentMethodTypeEnum.COD)
                {
                    await tx.CommitAsync();
                    return new PaymentInitiateResponseDTO
                    {
                        PaymentId = payment.PaymentId,
                        PaymentUrl = string.Empty,
                        Status = PaymentStatusEnum.Pending.ToString()
                    };
                }

                // Non-COD: create checkout session (S2S), save URL, log provider ref
                var envId = ResolveEnvironmentId();
                var providerId = MapProviderIdForMethodType(methodType)
                                 ?? throw new InvalidOperationException($"No provider mapping for {methodType}.");
                var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                         ?? throw new InvalidOperationException($"Active config not found for ProviderId={providerId}, EnvironmentId={envId}.");

                var checkout = await _paymentGatewaySimulator.CreateCheckoutSessionAsync(
                    payment.PaymentId, request.Amount, request.Currency, cfg.Id);

                await _paymentRepository.UpdatePaymentUrlAsync(payment.PaymentId, checkout.CheckoutUrl);

                var rawCheckout = JsonSerializer.Serialize(checkout);

                await CreateTransactionWithGatewayResponseAsync(
                    paymentId: payment.PaymentId,
                    amount: request.Amount,
                    gatewayRawResponse: rawCheckout,
                    statusCode: "102",
                    message: "Checkout session created",
                    errorMessage: null,
                    isRefund: false,
                    refundId: null,
                    providerConfigurationId: cfg.Id,
                    providerReferenceId: checkout.ProviderOrderId);

                await tx.CommitAsync();

                return new PaymentInitiateResponseDTO
                {
                    PaymentId = payment.PaymentId,
                    PaymentUrl = checkout.CheckoutUrl,
                    Status = PaymentStatusEnum.Pending.ToString()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentStatusResponseDTO?> GetPaymentStatusAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(paymentId);
            if (payment is null) return null;

            return new PaymentStatusResponseDTO
            {
                PaymentId = payment.PaymentId,
                Status = payment.PaymentStatus.Name.ToString(),
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethodTypeMaster.Name.ToString()
            };
        }

        public async Task<PaymentInitiateResponseDTO?> RetryPaymentAsync(Guid paymentId, PaymentMethodTypeEnum? methodOverride)
        {
            var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(paymentId);
            if (payment == null)
                return null;

            var effectiveMethod = methodOverride ?? (PaymentMethodTypeEnum)payment.PaymentMethodTypeId;

            // If override requested and different → update payment method + UPM
            if (methodOverride.HasValue && (int)effectiveMethod != payment.PaymentMethodTypeId)
            {
                Guid? newUpmId = null;
                if (effectiveMethod != PaymentMethodTypeEnum.COD)
                {
                    var methods = await _userPaymentMethodRepository.GetActivePaymentMethodsByUserAsync(payment.UserId);
                    var existing = methods.FirstOrDefault(m => m.MethodTypeId == (int)effectiveMethod);
                    if (existing != null)
                        newUpmId = existing.PaymentMethodId;
                }

                await _paymentRepository.UpdatePaymentMethodTypeAsync(payment.PaymentId, (int)effectiveMethod, newUpmId);
                payment.PaymentMethodTypeId = (int)effectiveMethod;
            }

            // COD → no PG; keep Pending; log manual/pending txn
            if (effectiveMethod == PaymentMethodTypeEnum.COD)
            {
                using var txCod = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, (int)PaymentStatusEnum.Pending);
                    await txCod.CommitAsync();
                    return new PaymentInitiateResponseDTO
                    {
                        PaymentId = payment.PaymentId,
                        PaymentUrl = null,
                        Status = PaymentStatusEnum.Pending.ToString()
                    };
                }
                catch
                {
                    await txCod.RollbackAsync();
                    throw;
                }
            }

            // Non-COD → create a fresh checkout session
            using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var envId = ResolveEnvironmentId();
                var providerId = MapProviderIdForMethodType(effectiveMethod)
                                 ?? throw new InvalidOperationException($"No provider mapping for {effectiveMethod}.");
                var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                         ?? throw new InvalidOperationException($"Active config not found for ProviderId={providerId}, EnvironmentId={envId}.");

                var checkout = await _paymentGatewaySimulator.CreateCheckoutSessionAsync(
                    payment.PaymentId, payment.Amount, payment.Currency, cfg.Id);

                await _paymentRepository.UpdatePaymentUrlAsync(payment.PaymentId, checkout.CheckoutUrl);

                var rawCheckout = JsonSerializer.Serialize(checkout);
                await CreateTransactionWithGatewayResponseAsync(
                    paymentId: payment.PaymentId,
                    amount: payment.Amount,
                    gatewayRawResponse: rawCheckout,
                    statusCode: "102",
                    message: "Retry checkout session created",
                    errorMessage: null,
                    isRefund: false,
                    refundId: null,
                    providerConfigurationId: cfg.Id,
                    providerReferenceId: checkout.ProviderOrderId);

                await _paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, (int)PaymentStatusEnum.Pending);
                await tx.CommitAsync();
                return new PaymentInitiateResponseDTO
                {
                    PaymentId = payment.PaymentId,
                    PaymentUrl = checkout.CheckoutUrl,
                    Status = PaymentStatusEnum.Pending.ToString()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelPaymentAsync(Guid paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(paymentId);
                if (payment == null) return false;

                if (!IsValidPaymentStatusTransition(payment.PaymentStatus.Name, PaymentStatusEnum.Canceled))
                    throw new InvalidOperationException($"Invalid payment status transition from {payment.PaymentStatus.Name} to Canceled");

                await _paymentRepository.UpdatePaymentStatusAsync(paymentId, (int)PaymentStatusEnum.Canceled);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentStatusResponseDTO>> GetPendingPaymentsAsync()
        {
            var list = await _paymentRepository.GetAllPendingPaymentsAsync();
            return list.Select(p => new PaymentStatusResponseDTO
            {
                PaymentId = p.PaymentId,
                Status = p.PaymentStatus.Name.ToString(),
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentMethod = p.PaymentMethodTypeMaster.Name.ToString()
            });
        }

        public async Task<int> ProcessPendingPaymentsAsync()
        {
            var pendings = await _paymentRepository.GetAllPendingPaymentsForRetryAsync();
            int processed = 0;

            foreach (var payment in pendings)
            {
                using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var envId = ResolveEnvironmentId();
                    var providerId = MapProviderIdForMethodType((PaymentMethodTypeEnum)payment.PaymentMethodTypeId)
                                     ?? throw new InvalidOperationException(
                                         $"No provider mapping for method {(PaymentMethodTypeEnum)payment.PaymentMethodTypeId}.");
                    var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                             ?? throw new InvalidOperationException(
                                 $"Active config not found for ProviderId={providerId}, EnvironmentId={envId}.");

                    // Query provider for the current payment status using the provider reference
                    var pg = await _paymentGatewaySimulator.GetPaymentDetailsAsync(payment.PaymentId.ToString());

                    // transaction + gateway response
                    var raw = JsonSerializer.Serialize(pg);
                    await CreateTransactionWithGatewayResponseAsync(
                        paymentId: payment.PaymentId,
                        amount: pg.Amount > 0 ? pg.Amount : payment.Amount,
                        gatewayRawResponse: raw,
                        statusCode: pg.StatusCode,
                        message: pg.Message,
                        errorMessage: pg.ErrorMessage,
                        isRefund: false,
                        refundId: null,
                        providerConfigurationId: cfg.Id,
                        providerReferenceId: pg.TransactionId
                    );

                    // Map provider status → payment status, then update if valid from Pending
                    var currentStatus = PaymentStatusEnum.Pending; // these are pending payments by query definition
                    var newStatus = MapPaymentStatus(pg.Status);

                    if (IsValidPaymentStatusTransition(currentStatus, newStatus))
                    {
                        await _paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, (int)newStatus);
                    }

                    await tx.CommitAsync();
                    processed++;
                }
                catch
                {
                    await tx.RollbackAsync();
                }
            }

            return processed;
        }

        public async Task<bool> HandlePaymentWebhookAsync(PaymentWebhookEventDTO payload)
        {
            if (payload is null)
                return false;

            if (!payload.PaymentId.HasValue || payload.PaymentId.Value == Guid.Empty)
                return false;

            if (string.IsNullOrWhiteSpace(payload.Status))
                return false;

            var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(payload.PaymentId.Value);
            if (payment is null)
                return false;

            if (payment.PaymentMethodTypeId == (int)PaymentMethodTypeEnum.COD)
                return false; // no PG webhooks expected

            var envId = ResolveEnvironmentId();
            var providerId = MapProviderIdForMethodType((PaymentMethodTypeEnum)payment.PaymentMethodTypeId)
                             ?? throw new InvalidOperationException("Cannot resolve provider.");
            var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                     ?? throw new InvalidOperationException("Active PG configuration not found.");

            var raw = JsonSerializer.Serialize(payload);

            await CreateTransactionWithGatewayResponseAsync(
                paymentId: payment.PaymentId,
                amount: payload.Amount ?? payment.Amount,
                gatewayRawResponse: raw,
                statusCode: payload.StatusCode ?? string.Empty,
                message: payload.Message ?? string.Empty,
                errorMessage: payload.ErrorMessage,
                isRefund: false,
                refundId: null,
                providerConfigurationId: cfg.Id,
                providerReferenceId: payload.TransactionId.ToString());

            var newStatus = MapPaymentStatus(payload.Status);
            if (IsValidPaymentStatusTransition(payment.PaymentStatus.Name, newStatus))
            {
                await _paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, (int)newStatus);
            }

            return true;
        }

        public async Task<PaymentTransactionResponseDTO?> GetPaymentTransactionsAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(paymentId);
            if (payment == null)
                return null;

            PaymentTransactionResponseDTO paymentTransactionResponseDTO = new PaymentTransactionResponseDTO();
            paymentTransactionResponseDTO.PaymentId = paymentId;
            paymentTransactionResponseDTO.PaymentStatus = payment.PaymentStatus.Name.ToString();

            var paymentTransactions = await _transactionRepository.GetTransactionsByPaymentIdAsync(paymentId);

            foreach (var transaction in paymentTransactions)
            {
                TransactionResponseDTO transactionResponseDTO = new TransactionResponseDTO()
                {
                    TransactionId = transaction.TransactionId,
                    TransactionStatus = transaction.TransactionStatus.Name.ToString(),
                    Amount = transaction.Amount,
                    Message = transaction.ErrorMessage,
                    PerformedBy = transaction.PerformedBy,
                    TransactionDate = transaction.CreatedAt,
                    GatewayTransactionId = transaction.GatewayTransactionId
                };

                paymentTransactionResponseDTO.TransactionResponses.Add(transactionResponseDTO);
            }

            return paymentTransactionResponseDTO;
        }
        #region Helpers

        private async Task<Guid?> ResolveUserPaymentMethodIdOrCreateAsync(Guid userId, PaymentMethodTypeEnum methodType)
        {
            if (methodType == PaymentMethodTypeEnum.COD)
                return null;

            var methods = await _userPaymentMethodRepository.GetActivePaymentMethodsByUserAsync(userId);
            var existing = methods.FirstOrDefault(pm => pm.MethodTypeId == (int)methodType);
            if (existing != null)
                return existing.PaymentMethodId;

            return null;
        }

        private int? MapProviderIdForMethodType(PaymentMethodTypeEnum methodType) => methodType switch
        {
            PaymentMethodTypeEnum.UPI => (int)GatewayProviderEnum.Razorpay, // Razorpay
            PaymentMethodTypeEnum.NetBanking => (int)GatewayProviderEnum.Razorpay, // Razorpay
            PaymentMethodTypeEnum.CreditCard => (int)GatewayProviderEnum.Stripe, // Stripe
            PaymentMethodTypeEnum.DebitCard => (int)GatewayProviderEnum.Stripe, // Stripe
            PaymentMethodTypeEnum.Wallet => (int)GatewayProviderEnum.Paytm, // Paytm
            PaymentMethodTypeEnum.COD => null, // no PG
            _ => (int)GatewayProviderEnum.Other // Other
        };

        private int ResolveEnvironmentId()
        {
            var envStr = _configuration["Payments:Environment"]?.Trim() ?? "Sandbox";

            return envStr.ToLowerInvariant() switch
            {
                "sandbox" => (int)EnvironmentTypeEnum.Sandbox,
                "live" => (int)EnvironmentTypeEnum.Live,
                "uat" => (int)EnvironmentTypeEnum.UAT,
                _ => (int)EnvironmentTypeEnum.Sandbox
            };
        }

        private PaymentStatusEnum MapPaymentStatus(string pgStatus) => pgStatus switch
        {
            "Success" => PaymentStatusEnum.Completed,
            "Pending" => PaymentStatusEnum.Pending,
            "Failed" => PaymentStatusEnum.Failed,
            "Cancelled" => PaymentStatusEnum.Canceled,
            _ => PaymentStatusEnum.Pending
        };

        private bool IsValidPaymentStatusTransition(PaymentStatusEnum current, PaymentStatusEnum next)
        {
            if (current == PaymentStatusEnum.Pending &&
                (next == PaymentStatusEnum.Completed || next == PaymentStatusEnum.Failed || next == PaymentStatusEnum.Canceled))
                return true;

            if (current == PaymentStatusEnum.Failed && next == PaymentStatusEnum.Pending)
                return true;

            return false;
        }

        private async Task<Transaction> CreateTransactionWithGatewayResponseAsync(
            Guid paymentId,
            decimal amount,
            string gatewayRawResponse,
            string statusCode,
            string message,
            string? errorMessage,
            bool isRefund,
            Guid? refundId,
            int? providerConfigurationId,
            string? providerReferenceId = null)
        {
            var transactionStatus = statusCode switch
            {
                "200" => (int)TransactionStatusEnum.Success,
                "102" => (int)TransactionStatusEnum.Pending,
                "500" => (int)TransactionStatusEnum.Failed,
                "400" => (int)TransactionStatusEnum.Canceled,
                _ => (int)TransactionStatusEnum.Pending
            };

            var transaction = new Transaction
            {
                PaymentId = paymentId,
                RefundId = refundId,
                TransactionStatusId = transactionStatus,
                PaymentProviderConfigurationId = providerConfigurationId ?? 0,
                Amount = amount,
                GatewayTransactionId = providerReferenceId ?? Guid.NewGuid().ToString(),
                ErrorMessage = errorMessage,
                PerformedBy = "PaymentGatewaySimulator",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _transactionRepository.CreateTransactionAsync(transaction);

            var gr = new GatewayResponse
            {
                PaymentId = paymentId,
                RefundId = refundId,
                TransactionId = created.TransactionId,
                RawResponse = gatewayRawResponse,
                StatusCode = statusCode,
                Message = message,
                ErrorMessage = errorMessage,
                ReceivedAt = DateTime.UtcNow
            };

            await _gatewayResponseRepository.AddGatewayResponseAsync(gr);
            return created;
        }

        #endregion
    }
}
