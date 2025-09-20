using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> InitiatePaymentAsync(Guid orderId, Guid userId, decimal amount, string currency, Guid? userPaymentMethodId, int paymentMethodTypeId, string? paymentUrl = null)
        {
            var utcNow = DateTime.UtcNow;

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                PaymentMethodTypeId = paymentMethodTypeId,
                UserPaymentMethodId = userPaymentMethodId,
                Amount = amount,
                Currency = currency,
                PaymentStatusId = (int)PaymentStatusEnum.Pending,
                PaymentUrl = paymentUrl,
                RetryCount = 0,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task UpdatePaymentStatusAsync(Guid paymentId, int paymentStatusId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            payment.PaymentStatusId = paymentStatusId;
            payment.UpdatedAt = DateTime.UtcNow;

            if (paymentStatusId == (int)TransactionStatusEnum.Failed || paymentStatusId == (int)TransactionStatusEnum.Pending)
            {
                payment.RetryCount += 1;
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<Payment?> GetPaymentByOrderAsync(Guid orderId, Guid userId)
        {
            return await _context.Payments.AsNoTracking()
                .Include(p => p.UserPaymentMethod)
                .Include(p => p.PaymentStatus)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.UserId == userId);
        }

        public async Task<Payment?> GetPaymentByPaymentIdAsync(Guid paymentId)
        {
            return await _context.Payments.AsNoTracking()
                .Include(p => p.UserPaymentMethod)
                .Include(p => p.PaymentStatus)
                .Include(p => p.Transactions)
                .Include(p => p.PaymentMethodTypeMaster)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<IEnumerable<Payment>> GetAllPendingPaymentsAsync()
        {
            // Payments with Pending status
            return await _context.Payments.AsNoTracking()
                .Include(p => p.PaymentStatus)
                .Include(p => p.Transactions)
                .Include(p => p.PaymentMethodTypeMaster)
                .Where(p => p.PaymentStatusId == (int)PaymentStatusEnum.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllPendingPaymentsForRetryAsync()
        {
            return await _context.Payments.AsNoTracking()
                .Where(p => p.PaymentStatusId == (int)PaymentStatusEnum.Pending
                        && p.PaymentMethodTypeId != (int)PaymentMethodTypeEnum.COD
                        && p.RetryCount < 5)
                .ToListAsync();
        }

        public async Task UpdatePaymentUrlAsync(Guid paymentId, string? paymentUrl)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment is null)
                throw new KeyNotFoundException($"Payment '{paymentId}' not found.");

            // Normalize: store null if empty/whitespace
            payment.PaymentUrl = string.IsNullOrWhiteSpace(paymentUrl) ? null : paymentUrl;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentMethodTypeAsync(Guid paymentId, int paymentMethodTypeId, Guid? userPaymentMethodId, bool clearPaymentUrlOnCod = true)
        {
            // Load payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment is null)
                throw new KeyNotFoundException($"Payment '{paymentId}' not found.");

            // Validate method type exists & active
            var isValidType = await _context.PaymentMethodTypes
                .AnyAsync(t => t.Id == paymentMethodTypeId && t.IsActive);
            if (!isValidType)
                throw new ArgumentException($"Invalid or inactive PaymentMethodTypeId '{paymentMethodTypeId}'.");

            // If a user instrument is supplied, validate it belongs to the same user (optional but recommended)
            if (userPaymentMethodId.HasValue)
            {
                var ok = await _context.UserPaymentMethods
                    .AnyAsync(m => m.PaymentMethodId == userPaymentMethodId.Value
                                   && m.IsActive
                                   && m.UserId == payment.UserId
                                   && m.MethodTypeId == paymentMethodTypeId);
                if (!ok)
                    throw new ArgumentException("UserPaymentMethodId is invalid for this user or method type.");
            }

            // Apply changes
            payment.PaymentMethodTypeId = paymentMethodTypeId;
            payment.UserPaymentMethodId = userPaymentMethodId;

            // If switching to COD, clear any stale PG URL (optional but avoids confusion)
            if (paymentMethodTypeId == (int)PaymentMethodTypeEnum.COD && clearPaymentUrlOnCod)
                payment.PaymentUrl = null;

            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}

