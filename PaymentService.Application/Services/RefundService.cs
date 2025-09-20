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
    public class RefundService : IRefundService
    {
        private readonly IValidator<RefundRequestDTO> _refundRequestValidator;
        private readonly IRefundRepository _refundRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGatewayResponseRepository _gatewayResponseRepository;
        private readonly IPaymentProviderConfigurationRepository _configRepository;
        private readonly PaymentGatewaySimulator _paymentGatewaySimulator;
        private readonly PaymentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public RefundService(
            IValidator<RefundRequestDTO> refundRequestValidator,
            IRefundRepository refundRepository,
            IPaymentRepository paymentRepository,
            ITransactionRepository transactionRepository,
            IGatewayResponseRepository gatewayResponseRepository,
            IPaymentProviderConfigurationRepository configRepository,
            PaymentDbContext dbContext,
            IConfiguration configuration)
        {
            _refundRequestValidator = refundRequestValidator;
            _refundRepository = refundRepository;
            _paymentRepository = paymentRepository;
            _transactionRepository = transactionRepository;
            _gatewayResponseRepository = gatewayResponseRepository;
            _configRepository = configRepository;
            _paymentGatewaySimulator = new PaymentGatewaySimulator();
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<RefundResponseDTO> InitiateRefundAsync(RefundRequestDTO request)
        {
            ValidationResult validationResult = await _refundRequestValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException($"Validation failed: {string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            // Load original payment (use PaymentId, not Order)
            var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(request.PaymentId)
                          ?? throw new KeyNotFoundException("Payment not found");

            // Prevent over-refund (include Pending + Completed)
            var existingRefunds = await _refundRepository.GetRefundsByPaymentIdAsync(request.PaymentId);
            decimal totalRefunded = existingRefunds
                .Where(r => r.RefundStatusId == (int)RefundStatusEnum.Completed || r.RefundStatusId == (int)RefundStatusEnum.Pending)
                .Sum(r => r.RefundAmount);

            if (request.RefundAmount + totalRefunded > payment.Amount)
                throw new ArgumentException("Refund amount exceeds remaining refundable amount");

            // Decide route:
            // If client passed RefundMethodTypeId, use it; otherwise infer:
            //   - COD payments → Manual (manual)
            //   - others → Original (PG)
            var inferredRoute = payment.PaymentMethodTypeId == (int)PaymentMethodTypeEnum.COD
                ? RefundMethodTypeEnum.Manual
                : RefundMethodTypeEnum.Original;

            using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Create refund in Pending
                var refund = await _refundRepository.InitiateRefundAsync(
                    request.PaymentId,
                    request.OriginalTransactionId,
                    (int)inferredRoute,
                    request.RefundAmount,
                    request.Reason ?? string.Empty,
                    request.InitiatedBy);

                // COD → no PG; log manual/pending transaction and leave refund Pending
                if (payment.PaymentMethodTypeId == (int)PaymentMethodTypeEnum.COD)
                {
                    var rawManual = JsonSerializer.Serialize(new
                    {
                        Route = "COD",
                        Note = "Manual refund; no PG involved"
                    });

                    await tx.CommitAsync();

                    return new RefundResponseDTO
                    {
                        RefundId = refund.RefundId,
                        Status = RefundStatusEnum.Pending.ToString(),
                        RefundAmount = refund.RefundAmount,
                        RefundMethod = ((RefundMethodTypeEnum)refund.RefundMethodTypeId).ToString()
                    };
                }

                // Non-COD → call PG (S2S)
                var envId = ResolveEnvironmentId();
                var providerId = MapProviderIdForMethodType((PaymentMethodTypeEnum)payment.PaymentMethodTypeId)
                                 ?? throw new InvalidOperationException($"No provider mapping for method {(PaymentMethodTypeEnum)payment.PaymentMethodTypeId}.");
                var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                         ?? throw new InvalidOperationException($"No active gateway configuration for ProviderId={providerId}, EnvironmentId={envId}.");

                var pgResponse = await _paymentGatewaySimulator.ProcessRefundAsync(
                    request.OriginalTransactionId?.ToString() ?? "",
                    request.RefundAmount);

                var rawResponse = JsonSerializer.Serialize(pgResponse);

                var refundTxn = await CreateTransactionWithGatewayResponseAsync(
                    paymentId: payment.PaymentId,
                    amount: request.RefundAmount,
                    gatewayRawResponse: rawResponse,
                    statusCode: pgResponse.StatusCode,
                    message: pgResponse.Message,
                    errorMessage: pgResponse.ErrorMessage,
                    isRefund: true,
                    refundId: refund.RefundId,
                    providerConfigurationId: cfg.Id,
                    providerReferenceId: pgResponse.TransactionId);

                var refundStatus = MapRefundStatus(pgResponse.Status);

                if (!IsValidRefundStatusTransition(refund.RefundStatusId, (int)refundStatus))
                    throw new InvalidOperationException($"Invalid refund status transition for refundId {refund.RefundId}");

                await _refundRepository.UpdateRefundStatusAsync(
                    refund.RefundId,
                    (int)refundStatus,
                    processedAt: refundStatus is RefundStatusEnum.Completed or RefundStatusEnum.Rejected ? DateTime.UtcNow : null,
                    refundTransactionId: refundTxn.TransactionId);

                await tx.CommitAsync();

                return new RefundResponseDTO
                {
                    RefundId = refund.RefundId,
                    Status = refundStatus.ToString(),
                    RefundAmount = refund.RefundAmount,
                    RefundMethod = ((RefundMethodTypeEnum)refund.RefundMethodTypeId).ToString()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<RefundResponseDTO?> GetRefundStatusAsync(Guid refundId)
        {
            var refund = await _refundRepository.GetRefundsByRefundIdAsync(refundId);
            if (refund == null) return null;

            return new RefundResponseDTO
            {
                RefundId = refund.RefundId,
                Status = refund.RefundStatus.Name.ToString(),
                RefundAmount = refund.RefundAmount,
                RefundMethod = refund.RefundMethodTypeMaster.Name.ToString()
            };
        }

        public async Task<RefundQueryResponseDTO> SearchRefundsAsync(RefundFilterRequestDTO request)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var (total, items) = await _refundRepository.GetRefundsByFilterAsync(
                request.Status,
                request.RefundMethodType,
                request.FromDate,
                request.ToDate,
                request.PaymentId,
                request.UserId,
                pageNumber,
                pageSize);

            var list = items.Select(r => new RefundListItemDTO
            {
                RefundId = r.RefundId,
                PaymentId = r.PaymentId,
                Status = r.RefundStatus.Name.ToString(),
                RefundMethod = r.RefundMethodTypeMaster.Name.ToString(),
                RefundAmount = r.RefundAmount,
                CreatedAt = r.CreatedAt,
                ProcessedAt = r.ProcessedAt,
                InitiatedBy = r.InitiatedBy
            });

            return new RefundQueryResponseDTO
            {
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = list.ToList()
            };
        }

        public async Task<int> ProcessPendingRefundsAsync()
        {
            var refundPendings = await _refundRepository.GetAllPendingRefundsForRetryAsync();
            int processed = 0;

            foreach (var refund in refundPendings)
            {
                using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Need payment to decide COD/provider
                    var payment = await _paymentRepository.GetPaymentByPaymentIdAsync(refund.PaymentId);
                    if (payment is null) { await tx.RollbackAsync(); continue; }

                    // COD → skip PG; keep Pending (manual)
                    if (payment.PaymentMethodTypeId == (int)PaymentMethodTypeEnum.COD)
                    {
                        await tx.CommitAsync(); // nothing to do automatically
                        continue;
                    }

                    var envId = ResolveEnvironmentId();
                    var providerId = MapProviderIdForMethodType((PaymentMethodTypeEnum)payment.PaymentMethodTypeId)
                                     ?? throw new InvalidOperationException($"No provider mapping for method {(PaymentMethodTypeEnum)payment.PaymentMethodTypeId}.");
                    var cfg = await _configRepository.GetActiveConfigurationByProviderAsync(providerId, envId)
                             ?? throw new InvalidOperationException($"Active PG config not found for ProviderId={providerId}, EnvironmentId={envId}.");

                    var pg = await _paymentGatewaySimulator.ProcessRefundAsync(
                        refund.PaymentTransactionId?.ToString() ?? "", refund.RefundAmount);

                    var raw = JsonSerializer.Serialize(pg);

                    await CreateTransactionWithGatewayResponseAsync(
                        paymentId: refund.PaymentId,
                        amount: refund.RefundAmount,
                        gatewayRawResponse: raw,
                        statusCode: pg.StatusCode,
                        message: pg.Message,
                        errorMessage: pg.ErrorMessage,
                        isRefund: true,
                        refundId: refund.RefundId,
                        providerConfigurationId: cfg.Id,
                        providerReferenceId: pg.TransactionId);

                    var newStatus = MapRefundStatus(pg.Status);

                    await _refundRepository.UpdateRefundStatusAsync(
                        refund.RefundId,
                        (int)newStatus,
                        processedAt: newStatus is RefundStatusEnum.Completed or RefundStatusEnum.Rejected ? DateTime.UtcNow : null);

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

        public async Task<RefundTransactionResponseDTO?> GetRefundTransactionsAsync(Guid refundId)
        {
            //Get the Refund Status based on Refund Id
            var refund = await _refundRepository.GetRefundsByRefundIdAsync(refundId);
            if (refund == null)
                return null;

            RefundTransactionResponseDTO refundTransactionResponseDTO = new RefundTransactionResponseDTO();
            refundTransactionResponseDTO.RefundId = refundId;
            refundTransactionResponseDTO.RefundStatus = refund.RefundStatus.Name.ToString();

            var refundTransactions = await _transactionRepository.GetTransactionsByRefundIdAsync(refundId);

            foreach (var transaction in refundTransactions)
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

                refundTransactionResponseDTO.TransactionResponses.Add(transactionResponseDTO);
            }

            return refundTransactionResponseDTO;
        }

        public async Task<ManualRefundProcessResponseDTO> ProcessRefundManuallyAsync(ManualRefundProcessRequestDTO request)
        {
            var refund = await _refundRepository.GetRefundsByRefundIdAsync(request.RefundId)
                ?? throw new KeyNotFoundException("Refund not found");

            // Validate status
            if (refund.RefundStatusId != (int)RefundStatusEnum.Pending)
                throw new InvalidOperationException("Only pending refunds can be processed manually");

            // Validate amount matches requested refund
            if (request.Amount != refund.RefundAmount)
                throw new InvalidOperationException("Amount must match the refund amount");

            // Determine final method type
            var methodTypeId = request.MethodTypeOverride.HasValue
                ? (int)request.MethodTypeOverride.Value
                : refund.RefundMethodTypeId;

            // Only allow Manual / BankTransfer / Wallet for manual processing
            if (!new[] { (int)RefundMethodTypeEnum.Manual, (int)RefundMethodTypeEnum.BankTransfer, (int)RefundMethodTypeEnum.Wallet }
                .Contains(methodTypeId))
                throw new InvalidOperationException("Manual processing allowed only for Manual, BankTransfer, or Wallet methods");

            // Begin transaction for atomicity
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Create transaction record for manual settlement
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    PaymentId = refund.PaymentId,
                    TransactionStatusId = (int)TransactionStatusEnum.Success,
                    Amount = request.Amount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PerformedBy = request.PerformedBy.ToString(),
                    GatewayTransactionId = request.SettlementReference,
                    ErrorMessage = request.Notes
                };

                await _transactionRepository.CreateTransactionAsync(transaction);

                // Update refund record
                refund.RefundStatusId = (int)RefundStatusEnum.Completed;
                refund.RefundMethodTypeId = methodTypeId;
                refund.RefundTransactionId = transaction.TransactionId;
                refund.UpdatedAt = DateTime.UtcNow;
                refund.ProcessedAt = request.ProcessedAtUtc ?? DateTime.UtcNow;
                refund.Reason = request.Notes;

                await _refundRepository.UpdateAsync(refund);

                // Commit DB transaction
                await dbTransaction.CommitAsync();

                return new ManualRefundProcessResponseDTO
                {
                    RefundId = refund.RefundId,
                    PaymentId = refund.PaymentId,
                    Status = RefundStatusEnum.Completed.ToString(),
                    RefundAmount = refund.RefundAmount,
                    RefundMethod = ((RefundMethodTypeEnum)methodTypeId).ToString(),
                    ProcessedAt = refund.ProcessedAt,
                    TransactionId = transaction.TransactionId,
                    SettlementReference = request.SettlementReference,
                    PerformedBy = request.PerformedBy,
                    Notes = request.Notes
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        #region Helpers

        private RefundStatusEnum MapRefundStatus(string pgStatus) => pgStatus switch
        {
            "Success" => RefundStatusEnum.Completed,
            "Pending" => RefundStatusEnum.Pending,
            "Failed" => RefundStatusEnum.Failed,
            "Cancelled" => RefundStatusEnum.Rejected,
            _ => RefundStatusEnum.Pending
        };

        private bool IsValidRefundStatusTransition(int currentStatusId, int newStatusId)
        {
            if (currentStatusId == (int)RefundStatusEnum.Pending &&
                (newStatusId == (int)RefundStatusEnum.Completed ||
                 newStatusId == (int)RefundStatusEnum.Failed ||
                 newStatusId == (int)RefundStatusEnum.Rejected))
                return true;

            return false;
        }

        private int ResolveEnvironmentId()
        {
            var envStr = _configuration["Payments:Environment"]?.Trim() ?? "Sandbox";
            if (int.TryParse(envStr, out var asId)) return asId;

            return envStr.ToLowerInvariant() switch
            {
                "sandbox" => (int)EnvironmentTypeEnum.Sandbox,
                "live" => (int)EnvironmentTypeEnum.Live,
                "uat" => (int)EnvironmentTypeEnum.UAT,
                _ => (int)EnvironmentTypeEnum.Sandbox
            };
        }

        private int? MapProviderIdForMethodType(PaymentMethodTypeEnum methodType) => methodType switch
        {
            PaymentMethodTypeEnum.UPI => (int)GatewayProviderEnum.Razorpay, // Razorpay
            PaymentMethodTypeEnum.NetBanking => (int)GatewayProviderEnum.Razorpay, // Razorpay
            PaymentMethodTypeEnum.CreditCard => (int)GatewayProviderEnum.Stripe, // Stripe
            PaymentMethodTypeEnum.DebitCard => (int)GatewayProviderEnum.Stripe, // Stripe
            PaymentMethodTypeEnum.Wallet => (int)GatewayProviderEnum.Paytm, // Paytm
            PaymentMethodTypeEnum.COD => null,
            _ => (int)GatewayProviderEnum.Other // Other
        };

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

            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);

            var gatewayResponse = new GatewayResponse
            {
                PaymentId = paymentId,
                RefundId = refundId,
                TransactionId = createdTransaction.TransactionId,
                RawResponse = gatewayRawResponse,
                StatusCode = statusCode,
                Message = message,
                ErrorMessage = errorMessage,
                ReceivedAt = DateTime.UtcNow
            };

            await _gatewayResponseRepository.AddGatewayResponseAsync(gatewayResponse);
            return createdTransaction;
        }

        #endregion
    }
}
