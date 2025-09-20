using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly OrderDbContext _dbContext;

        public CancellationRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get a cancellation by its unique ID 
        public async Task<Cancellation?> GetByIdAsync(Guid cancellationId)
        {
            // Loads all relevant navigation properties for full details (read-only)
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationItems)
                    .ThenInclude(ci => ci.OrderItem)
                .Include(c => c.CancellationStatus)
                .Include(c => c.Reason)
                .Include(c => c.Policy)
                .FirstOrDefaultAsync(c => c.Id == cancellationId && !c.IsDeleted);
        }

        // Get a cancellation by orderId
        public async Task<List<Cancellation>> GetCancellationsByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationItems)
                    .ThenInclude(ci => ci.OrderItem)
                .Include(c => c.CancellationStatus)
                .Include(c => c.Reason)
                .Include(c => c.Policy)
                .Where(c => c.OrderId== orderId && !c.IsDeleted).ToListAsync();
        }
        
        // Add a new cancellation request 
        public async Task<Cancellation?> AddAsync(Cancellation cancellation)
        {
            if (cancellation == null)
                throw new ArgumentNullException(nameof(cancellation));

            cancellation.RequestedAt = DateTime.UtcNow;
            cancellation.IsDeleted = false;
            cancellation.CancellationStatusId = (int)CancellationStatusEnum.Pending;

            await _dbContext.Cancellations.AddAsync(cancellation);
            await _dbContext.SaveChangesAsync();
            return cancellation;
        }

        // Update an existing cancellation (Customer can update before admin review)
        public async Task<Cancellation?> UpdateAsync(Cancellation cancellation)
        {
            if (cancellation == null)
                throw new ArgumentNullException(nameof(cancellation));

            var existing = await _dbContext.Cancellations
                .Include(c => c.CancellationItems)
                .FirstOrDefaultAsync(c => c.Id == cancellation.Id && !c.IsDeleted);

            if (existing == null)
                return null;

            // Only allow update if status is Pending (Requested)
            if (existing.CancellationStatusId != (int)CancellationStatusEnum.Pending)
            {
                // Option 1: Return null to indicate update not allowed
                return null;

                // Option 2: throw new InvalidOperationException("Cannot update cancellation in current status");
            }

            // Update allowed fields
            existing.Remarks = cancellation.Remarks;
            existing.ReasonId = cancellation.ReasonId;
            existing.IsPartial = cancellation.IsPartial;
            existing.CancellationPolicyId = cancellation.CancellationPolicyId;

            // Remove items that are no longer present
            var incomingItemIds = cancellation.CancellationItems?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();
            var itemsToRemove = existing.CancellationItems.Where(ei => !incomingItemIds.Contains(ei.Id)).ToList();
            _dbContext.CancellationItems.RemoveRange(itemsToRemove);

            // Update or add items
            if (cancellation.CancellationItems != null)
            {
                foreach (var incomingItem in cancellation.CancellationItems)
                {
                    var existingItem = existing.CancellationItems.FirstOrDefault(ei => ei.Id == incomingItem.Id);
                    if (existingItem != null)
                    {
                        // Update existing item fields
                        existingItem.Quantity = incomingItem.Quantity;
                        existingItem.OrderItemId = incomingItem.OrderItemId;
                    }
                    else
                    {
                        // New item: attach to existing cancellation
                        incomingItem.CancellationId = existing.Id;
                        _dbContext.CancellationItems.Add(incomingItem);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // Delete (soft delete) a cancellation by ID (Customer request)
        public async Task DeleteAsync(Guid cancellationId)
        {
            var cancellation = await _dbContext.Cancellations.FindAsync(cancellationId);
            if (cancellation == null) return;

            // Only allow delete if status is Requested (Pending)
            if (cancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
            {
                // Option 1: silently return
                // return;

                // Option 2: throw exception to notify caller
                throw new InvalidOperationException("Cannot delete cancellation that is already processed.");
            }

            // Perform soft delete
            cancellation.IsDeleted = true;
            cancellation.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        // Returns list of OrderItem IDs that are already cancelled or have pending cancellation requests for the specified order.
        public async Task<List<Guid>> GetCancelledOrderItemIdsAsync(Guid orderId)
        {
            // Query cancellation items where parent cancellation belongs to the given order
            // and cancellation status is Pending or Approved
            var cancelledItemIds = await _dbContext.CancellationItems
                .Where(ci => ci.Cancellation.OrderId == orderId
                             && (ci.Cancellation.CancellationStatusId == (int)CancellationStatusEnum.Pending
                                 || ci.Cancellation.CancellationStatusId == (int)CancellationStatusEnum.Approved))
                .Select(ci => ci.OrderItemId)
                .Distinct()
                .ToListAsync();

            return cancelledItemIds;
        }

        // Get all cancellation items linked to a cancellation
        public async Task<List<CancellationItem>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId)
        {
            return await _dbContext.CancellationItems
                .AsNoTracking()
                .Where(ci => ci.CancellationId == cancellationId)
                .ToListAsync();
        }

        public async Task ApproveAsync(Cancellation updatedCancellation, OrderStatusEnum newOrderStatus, string? processedBy, string? Remarks)
        {
            if (updatedCancellation == null)
                throw new ArgumentNullException(nameof(updatedCancellation));

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Load existing cancellation for concurrency check
                var existingCancellation = await _dbContext.Cancellations
                    .Include(c => c.CancellationItems)
                    .Include(c => c.Order)
                        .ThenInclude(o => o.OrderItems)
                    .FirstOrDefaultAsync(c => c.Id == updatedCancellation.Id);

                if (existingCancellation == null)
                    throw new InvalidOperationException($"Cancellation {updatedCancellation.Id} not found.");

                if (existingCancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
                    throw new InvalidOperationException("Only Pending cancellations can be approved.");

                var order = existingCancellation.Order ?? throw new InvalidOperationException("Order related to cancellation not found.");

                // Update cancellation properties
                existingCancellation.PurchaseTotalAmount = updatedCancellation.PurchaseTotalAmount;
                existingCancellation.CancellationCharge = updatedCancellation.CancellationCharge;
                existingCancellation.TotalRefundAmount = updatedCancellation.TotalRefundAmount;
                existingCancellation.ProcessedBy = processedBy ?? "System";
                existingCancellation.ProcessedAt = DateTime.UtcNow;
                existingCancellation.ApprovedBy = processedBy ?? "System";
                existingCancellation.ApprovalRemarks = Remarks;
                existingCancellation.ApprovedAt = DateTime.UtcNow;
                existingCancellation.CancellationStatusId = (int)CancellationStatusEnum.Approved;

                // Update cancellation items amounts
                foreach (var updatedItem in updatedCancellation.CancellationItems)
                {
                    var existingItem = existingCancellation.CancellationItems.FirstOrDefault(ci => ci.Id == updatedItem.Id);
                    if (existingItem != null)
                    {
                        existingItem.PurchaseAmount = updatedItem.PurchaseAmount;
                        existingItem.RefundAmount = updatedItem.RefundAmount;
                        existingItem.CancellationCharge = updatedItem.CancellationCharge;
                    }
                    else
                    {
                        throw new InvalidOperationException($"CancellationItem {updatedItem.Id} not found.");
                    }
                }

                // Update order items status only for cancelled items
                var cancelledOrderItemIds = updatedCancellation.CancellationItems.Select(ci => ci.OrderItemId).ToHashSet();
                foreach (var orderItem in order.OrderItems)
                {
                    if (cancelledOrderItemIds.Contains(orderItem.Id))
                    {
                        orderItem.ItemStatusId = (int)OrderStatusEnum.Cancelled;
                    }
                }

                // Update order status
                order.OrderStatusId = (int)newOrderStatus;

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Reject a cancellation request (Admin)
        public async Task RejectAsync(Guid cancellationId, string rejectedBy, string? remarks = null)
        {
            var cancellation = await _dbContext.Cancellations.FindAsync(cancellationId);
            if (cancellation == null)
                throw new KeyNotFoundException("Cancellation not found.");

            // Only allow rejecting if current status is Requested (Pending)
            if (cancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
                throw new InvalidOperationException("Cancellation cannot be rejected in its current state.");

            // Update status and audit info
            cancellation.CancellationStatusId = (int)CancellationStatusEnum.Rejected;
            cancellation.RejectedBy = rejectedBy;
            cancellation.RejectionRemarks = remarks;
            cancellation.RejectedAt = DateTime.UtcNow;
            cancellation.ProcessedBy = rejectedBy;
            cancellation.ProcessedAt = cancellation.RejectedAt;

            _dbContext.Cancellations.Update(cancellation);
            await _dbContext.SaveChangesAsync();
        }

        // Check if a cancellation exists by ID
        public async Task<bool> ExistsAsync(Guid cancellationId)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .AnyAsync(c => c.Id == cancellationId && !c.IsDeleted);
        }

        //Filter cancellation request with optional parameters
        public async Task<(List<Cancellation> Items, int TotalCount)> GetCancellationsWithFiltersAsync(
                Guid? userId = null,
                CancellationStatusEnum? status = null,
                DateTime? fromDate = null,
                DateTime? toDate = null,
                int pageNumber = 1,
                int pageSize = 20)
        {
            // Base query filtering out soft-deleted records
            var query = _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationStatus)
                .Include(c => c.Policy)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            // Apply filters dynamically
            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.CancellationStatusId == (int)status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.RequestedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.RequestedAt <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting and paging
            var items = await query
                .OrderByDescending(c => c.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
