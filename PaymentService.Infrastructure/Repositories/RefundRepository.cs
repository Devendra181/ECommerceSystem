using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Repositories
{
    public class RefundRepository : IRefundRepository
    {
        private readonly PaymentDbContext _context;

        public RefundRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Refund> InitiateRefundAsync(Guid paymentId, Guid? originalTransactionId, int refundMethodTypeId, decimal amount, string reason, Guid initiatedBy)
        {
            var utcNow = DateTime.UtcNow;

            var refund = new Refund
            {
                RefundId = Guid.NewGuid(),

                PaymentId = paymentId,
                RefundMethodTypeId = refundMethodTypeId,
                PaymentTransactionId = originalTransactionId,
                RefundAmount = amount,
                Reason = reason,
                RefundStatusId = (int)RefundStatusEnum.Pending,
                RetryCount = 0,
                InitiatedBy = initiatedBy,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();

            return refund;
        }

        public async Task UpdateRefundStatusAsync(Guid refundId, int refundStatusId, DateTime? processedAt = null, Guid? refundTransactionId = null)
        {
            var refund = await _context.Refunds.FindAsync(refundId);
            if (refund == null)
                throw new KeyNotFoundException($"Refund with ID {refundId} not found.");

            refund.RefundStatusId = refundStatusId;
            refund.ProcessedAt = processedAt ?? refund.ProcessedAt;

            if (refundTransactionId.HasValue)
            {
                refund.RefundTransactionId = refundTransactionId.Value;
            }

            if (refundStatusId == (int)TransactionStatusEnum.Failed || refundStatusId == (int)TransactionStatusEnum.Pending)
            {
                refund.RetryCount += 1;
            }

            _context.Refunds.Update(refund);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Refund>> GetRefundsByPaymentIdAsync(Guid paymentId)
        {
            return await _context.Refunds.AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Payment)
                .Include(r => r.Transaction)
                .Where(r => r.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<Refund?> GetRefundsByRefundIdAsync(Guid refundId)
        {
            return await _context.Refunds
                .Include(r => r.RefundStatus)
                .Include(r => r.Payment)
                .Include(r => r.Transaction)
                .Include(r => r.RefundMethodTypeMaster)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefundId == refundId);
        }

        public async Task<IEnumerable<Refund>> GetAllPendingRefundsForRetryAsync()
        {
            return await _context.Refunds.AsNoTracking()
                .Where(r => r.RefundStatusId == (int)RefundStatusEnum.Pending
                        && r.RefundMethodTypeId != (int)RefundMethodTypeEnum.Manual
                        && r.RetryCount < 5)
                .ToListAsync();
        }

        public async Task UpdateAsync(Refund refund)
        {
            _context.Refunds.Update(refund);
            await _context.SaveChangesAsync();
        }

        public async Task<(int Total, List<Refund> Items)> GetRefundsByFilterAsync(
            RefundStatusEnum? status,
            RefundMethodTypeEnum? refundMethodType,
            DateTime? fromDateUtc,
            DateTime? toDateUtc,
            Guid? paymentId,
            Guid? userId,
            int pageNumber,
            int pageSize)
        {
            // Base query + includes used by the mapper (adjust nav names if needed)
            var query = _context.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)              // ensure Status.Name
                .Include(r => r.RefundMethodTypeMaster)    // ensure Method.Name
                .AsQueryable();

            // Filters (adjust property names if different)
            if (status.HasValue)
            {
                query = query.Where(r => r.RefundStatusId == (int)status.Value);
            }

            if (refundMethodType.HasValue)
            {
                query = query.Where(r => r.RefundMethodTypeId == (int)refundMethodType.Value);
            }

            if (fromDateUtc.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDateUtc.Value);
            }

            if (toDateUtc.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= toDateUtc.Value);
            }

            if (paymentId.HasValue)
                query = query.Where(r => r.PaymentId == paymentId.Value);

            if (userId.HasValue)
                query = query.Where(r => r.InitiatedBy == userId.Value);

            // Sorting (latest first)
            query = query.OrderByDescending(r => r.CreatedAt);

            // Total + paging
            var total = await query.CountAsync();
            var skip = (pageNumber <= 1 ? 0 : (pageNumber - 1) * pageSize);
            var items = await query.Skip(skip).Take(pageSize).ToListAsync();

            return (total, items);
        }
    }
}

