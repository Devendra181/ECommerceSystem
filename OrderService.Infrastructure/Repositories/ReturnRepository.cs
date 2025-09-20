using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly OrderDbContext _dbContext;

        public ReturnRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get a returns by its unique ID 
        public async Task<Return?> GetByIdAsync(Guid returnId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.OrderItem)
                .Include(r => r.ReturnStatus)
                .Include(r => r.Reason)
                .Include(r => r.Policy)
                .FirstOrDefaultAsync(r => r.Id == returnId && !r.IsDeleted);
        }

        // Get a returns by orderId
        public async Task<List<Return>> GetReturnsByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnItems)
                    .ThenInclude(ci => ci.OrderItem)
                .Include(r => r.ReturnStatus)
                .Include(r => r.Reason)
                .Include(r => r.Policy)
                .Where(r => r.OrderId == orderId && !r.IsDeleted).ToListAsync();
        }

        // Add a new return request 
        public async Task<Return?> AddAsync(Return returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            returnRequest.RequestedAt = DateTime.UtcNow;
            returnRequest.IsDeleted = false;
            returnRequest.ReturnStatusId = (int)ReturnStatusEnum.Pending;

            await _dbContext.Returns.AddAsync(returnRequest);
            await _dbContext.SaveChangesAsync();

            return returnRequest;
        }

        // Update an existing return (Customer can update before admin review)
        public async Task<Return?> UpdateAsync(Return returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            var existing = await _dbContext.Returns
                .Include(r => r.ReturnItems)
                .FirstOrDefaultAsync(r => r.Id == returnRequest.Id && !r.IsDeleted);

            if (existing == null)
                return null;

            if (existing.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                return null;

            // Update allowed fields
            existing.Remarks = returnRequest.Remarks;
            existing.ReasonId = returnRequest.ReasonId;
            existing.IsPartial = returnRequest.IsPartial;
            existing.ReturnPolicyId = returnRequest.ReturnPolicyId;

            // Remove items that are no longer present
            var incomingItemIds = returnRequest.ReturnItems?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();

            var itemsToRemove = existing.ReturnItems.Where(ei => !incomingItemIds.Contains(ei.Id)).ToList();
            _dbContext.ReturnItems.RemoveRange(itemsToRemove);

            // Update or add items
            if (returnRequest.ReturnItems != null)
            {
                foreach (var incomingItem in returnRequest.ReturnItems)
                {
                    var existingItem = existing.ReturnItems.FirstOrDefault(ei => ei.Id == incomingItem.Id);
                    if (existingItem != null)
                    {
                        // Update existing item fields
                        existingItem.Quantity = incomingItem.Quantity;
                        existingItem.OrderItemId = incomingItem.OrderItemId;
                        existingItem.Remarks = incomingItem.Remarks;
                    }
                    else
                    {
                        // New item: attach to existing return
                        incomingItem.ReturnId = existing.Id;
                        _dbContext.ReturnItems.Add(incomingItem);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(Guid returnId)
        {
            var returnRequest = await _dbContext.Returns.FindAsync(returnId);
            if (returnRequest == null)
                return;

            returnRequest.IsDeleted = true;
            returnRequest.DeletedAt = DateTime.UtcNow;

            _dbContext.Returns.Update(returnRequest);
            await _dbContext.SaveChangesAsync();
        }

        // Returns list of OrderItem IDs that are already returned or have pending cancellation requests for the specified order.
        public async Task<List<Guid>> GetReturnedOrderItemIdsAsync(Guid orderId)
        {
            // Return OrderItemIds from ReturnItems where parent Return belongs to the order
            // and return status is Pending or Approved
            var returnedItemIds = await _dbContext.ReturnItems
                .Where(ri => ri.Return.OrderId == orderId &&
                             (ri.Return.ReturnStatusId == (int)ReturnStatusEnum.Pending ||
                              ri.Return.ReturnStatusId == (int)ReturnStatusEnum.Approved))
                .Select(ri => ri.OrderItemId)
                .Distinct()
                .ToListAsync();

            return returnedItemIds;
        }

        // Get all return items linked to a return request
        public async Task<List<ReturnItem>> GetReturnItemsByReturnIdAsync(Guid returnId)
        {
            return await _dbContext.ReturnItems
                .AsNoTracking()
                .Where(ri => ri.ReturnId == returnId)
                .ToListAsync();
        }

        public async Task ApproveAsync(Return updatedReturn, string approvedBy, decimal refundableAmount, string? remarks = null)
        {
            if (updatedReturn == null)
                throw new ArgumentNullException(nameof(updatedReturn));

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingReturn = await _dbContext.Returns
                    .Include(r => r.ReturnItems)
                    .Include(r => r.Order)
                        .ThenInclude(o => o.OrderItems)
                    .FirstOrDefaultAsync(r => r.Id == updatedReturn.Id);

                if (existingReturn == null)
                    throw new InvalidOperationException($"Return request {updatedReturn.Id} not found.");

                if (existingReturn.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                    throw new InvalidOperationException("Only Pending returns can be approved.");

                var order = existingReturn.Order ?? throw new InvalidOperationException("Order related to return not found.");

                // Validate amounts
                if (updatedReturn.PurchaseTotalAmount < 0 || updatedReturn.RestockingFee < 0 || updatedReturn.TotalRefundableAmount < 0)
                    throw new InvalidOperationException("Amount values cannot be negative.");

                foreach (var item in updatedReturn.ReturnItems)
                {
                    if (item.PurchaseAmount < 0 || item.RefundAmount < 0)
                        throw new InvalidOperationException("Return item amounts cannot be negative.");
                }

                // Update return entity amounts & audit fields
                existingReturn.PurchaseTotalAmount = updatedReturn.PurchaseTotalAmount;
                existingReturn.RestockingFee = updatedReturn.RestockingFee;
                existingReturn.TotalRefundableAmount = updatedReturn.TotalRefundableAmount;
                existingReturn.ProcessedBy = approvedBy ?? "System";
                existingReturn.ProcessedAt = DateTime.UtcNow;
                existingReturn.ReturnStatusId = (int)ReturnStatusEnum.Approved;
                existingReturn.ApprovedBy = approvedBy;
                existingReturn.ApprovalRemarks = remarks;
                existingReturn.ApprovedAt = DateTime.UtcNow;

                // Update return items amounts
                foreach (var updatedItem in updatedReturn.ReturnItems)
                {
                    var existingItem = existingReturn.ReturnItems.FirstOrDefault(ri => ri.Id == updatedItem.Id);
                    if (existingItem != null)
                    {
                        existingItem.PurchaseAmount = updatedItem.PurchaseAmount;
                        existingItem.RefundAmount = updatedItem.RefundAmount;
                        existingItem.RestockingFee = updatedItem.RestockingFee;
                    }
                    else
                    {
                        throw new InvalidOperationException($"ReturnItem {updatedItem.Id} not found.");
                    }
                }

                // Update order items status only for returned items
                var returnedOrderItemIds = updatedReturn.ReturnItems.Select(ri => ri.OrderItemId).ToHashSet();
                foreach (var orderItem in order.OrderItems)
                {
                    if (returnedOrderItemIds.Contains(orderItem.Id))
                    {
                        orderItem.ItemStatusId = (int)OrderStatusEnum.Returned;
                    }
                }

                // Update order status
                order.OrderStatusId = updatedReturn.IsPartial ? (int)OrderStatusEnum.PartialReturned : (int)OrderStatusEnum.Returned;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RejectAsync(Guid returnId, string rejectedBy, string? remarks = null)
        {
            var returnRequest = await _dbContext.Returns.FindAsync(returnId);
            if (returnRequest == null)
                throw new KeyNotFoundException("Return request not found.");

            if (returnRequest.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Return request cannot be rejected in its current state.");

            returnRequest.ReturnStatusId = (int)ReturnStatusEnum.Rejected;
            returnRequest.RejectedBy = rejectedBy;
            returnRequest.RejectionRemarks = remarks;
            returnRequest.RejectedAt = DateTime.UtcNow;
            returnRequest.ProcessedBy = rejectedBy;
            returnRequest.ProcessedAt = returnRequest.RejectedAt;

            _dbContext.Returns.Update(returnRequest);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<bool> ExistsAsync(Guid returnId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .AnyAsync(r => r.Id == returnId && !r.IsDeleted);
        }

        public async Task<(List<Return> Items, int TotalCount)> GetReturnsWithFiltersAsync(
            Guid? userId = null,
            ReturnStatusEnum? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnStatus)
                .Include(r => r.Policy)
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(r => r.ReturnStatusId == (int)status.Value);

            if (fromDate.HasValue)
                query = query.Where(r => r.RequestedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.RequestedAt <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
