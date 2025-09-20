using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class RefundRepository : IRefundRepository
    {
        private readonly OrderDbContext _dbContext;

        public RefundRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Create a new Refund record
        public async Task<Refund> AddAsync(Refund refund)
        {
            if (refund == null)
                throw new ArgumentNullException(nameof(refund));

            refund.RefundDate = DateTime.UtcNow;
            refund.RefundStatusId = (int)RefundStatusEnum.Pending;
            refund.IsDeleted = false;

            await _dbContext.Refunds.AddAsync(refund);
            await _dbContext.SaveChangesAsync();

            return refund;
        }

        // Get Refund by Id including related entities
        public async Task<Refund?> GetByIdAsync(Guid refundId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.Cancellation)
                .Include(r => r.Return)
                .Include(r => r.Order)
                .Include(r => r.RefundStatus)
                .FirstOrDefaultAsync(r => r.Id == refundId && !r.IsDeleted);
        }

        // Update only the status and related audit fields of a Refund
        public async Task<Refund> UpdateStatusAsync(Guid refundId, RefundStatusEnum newStatus, string processedBy, string? remarks = null)
        {
            var refund = await _dbContext.Refunds.FindAsync(refundId);
            if (refund == null)
                throw new KeyNotFoundException($"Refund with Id {refundId} not found.");

            refund.RefundStatusId = (int)newStatus;
            refund.ProcessedBy = processedBy;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.Remarks = remarks;

            _dbContext.Refunds.Update(refund);
            await _dbContext.SaveChangesAsync();

            return refund;
        }

        // Soft delete a Refund by Id
        public async Task DeleteAsync(Guid refundId)
        {
            var refund = await _dbContext.Refunds.FindAsync(refundId);
            if (refund == null)
                return; // Or throw if preferred

            // Soft delete by setting IsDeleted flag and timestamp
            refund.IsDeleted = true;
            refund.DeletedAt = DateTime.UtcNow;

            _dbContext.Refunds.Update(refund);
            await _dbContext.SaveChangesAsync();
        }

        // Check if Refund exists by Id
        public async Task<bool> ExistsAsync(Guid refundId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .AnyAsync(r => r.Id == refundId && !r.IsDeleted);
        }

        // Get Refund by CancellationId
        public async Task<Refund?> GetByCancellationIdAsync(Guid cancellationId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .FirstOrDefaultAsync(r => r.CancellationId == cancellationId && !r.IsDeleted);
        }

        // Get Refund by ReturnId
        public async Task<Refund?> GetByReturnIdAsync(Guid returnId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId && !r.IsDeleted);
        }

        // Get Refunds by OrderId with pagination
        public async Task<(List<Refund> Items, int TotalCount)> GetByOrderIdAsync(Guid orderId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Where(r => r.OrderId == orderId && !r.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Get paginated Refunds with optional filters
        public async Task<(List<Refund> Items, int TotalCount)> GetRefundsWithFiltersAsync(
            Guid? userId = null,
            RefundStatusEnum? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.Order.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(r => r.RefundStatusId == (int)status.Value);

            if (fromDate.HasValue)
                query = query.Where(r => r.RefundDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.RefundDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
