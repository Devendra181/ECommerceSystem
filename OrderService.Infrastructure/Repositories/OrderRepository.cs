using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        // Define allowed transitions dictionary using OrderStatusEnum
        private static readonly Dictionary<OrderStatusEnum, List<OrderStatusEnum>> AllowedTransitions = new()
        {
            //Pending → Confirmed, Cancelled
            { OrderStatusEnum.Pending, new List<OrderStatusEnum> { OrderStatusEnum.Confirmed, OrderStatusEnum.Cancelled } },
            
            //Confirmed → Packed, Cancelled
            { OrderStatusEnum.Confirmed, new List<OrderStatusEnum> { OrderStatusEnum.Packed, OrderStatusEnum.Cancelled } },
            
            //Packed → Shipped, Cancelled
            { OrderStatusEnum.Packed, new List<OrderStatusEnum> { OrderStatusEnum.Shipped, OrderStatusEnum.Cancelled } },
            
            //Shipped → Delivered, Cancelled
            { OrderStatusEnum.Shipped, new List<OrderStatusEnum> { OrderStatusEnum.Delivered, OrderStatusEnum.Cancelled } },
            
            //Delivered → Returned
            { OrderStatusEnum.Delivered, new List<OrderStatusEnum> { OrderStatusEnum.Returned } },  
            
            //Cancelled & Returned → terminal
            { OrderStatusEnum.Cancelled, new List<OrderStatusEnum>() },  // Terminal state
            { OrderStatusEnum.Returned, new List<OrderStatusEnum>() }    // Terminal state
        };

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Add a new order (Customer)
        public async Task<Order?> AddAsync(Order order)
        {
            if (order == null) 
                throw new ArgumentNullException(nameof(order));

            await _dbContext.Orders.AddAsync(order);

            var history = new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OldStatusId = order.OrderStatusId, 
                NewStatusId = order.OrderStatusId,
                ChangedBy = "System", 
                Remarks = "Order created with initial status",
                ChangedAt = DateTime.UtcNow,
                Order = order
            };

            _dbContext.OrderStatusHistories.Add(history);

            await _dbContext.SaveChangesAsync();
            return order;
        }

        // Get paginated list of orders for a specific user (most recent first) [Customer]
        public async Task<List<Order>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Changes the status of the order with optional remarks, also records status history
        public async Task<bool> ChangeOrderStatusAsync(Guid orderId, OrderStatusEnum newStatusId, string? changedBy = null, string? remarks = null)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.OrderStatusId == (int)newStatusId)
                return true;

            var oldStatus = (OrderStatusEnum)order.OrderStatusId;

            if (!AllowedTransitions.TryGetValue(oldStatus, out var validNextStatuses) || !validNextStatuses.Contains(newStatusId))
            {
                throw new InvalidOperationException($"Transition from {oldStatus} to {newStatusId} is not allowed.");
            }

            order.OrderStatusId = (int)newStatusId;
            order.ModifiedAt = DateTime.Now;

            if (newStatusId == OrderStatusEnum.Confirmed)
            {
                order.DeliveryDate = DateTime.UtcNow;

                // Update all related OrderItems status to Confirmed
                foreach (var item in order.OrderItems)
                {
                    item.ItemStatusId = (int)OrderStatusEnum.Confirmed;
                }
            }

            if (newStatusId == OrderStatusEnum.Delivered)
            {
                order.DeliveryDate = DateTime.UtcNow;

                // Update all related OrderItems status to Delivered
                foreach (var item in order.OrderItems)
                {
                    item.ItemStatusId = (int)OrderStatusEnum.Delivered;
                }
            }

            var history = new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OldStatusId = (int)oldStatus,
                NewStatusId = (int)newStatusId,
                ChangedBy = changedBy,
                Remarks = remarks,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.OrderStatusHistories.Add(history);

            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts here
                throw new InvalidOperationException("The order was modified by another process. Please reload and try again.");
            }
        }


        // Gets the full status history for an order (Customer, Admin, Reporting)
        public async Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(Guid orderId)
        {
            return await _dbContext.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        // Delete an order by ID (Admin only)
        public async Task DeleteAsync(Guid orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }

        // Check if an order exists by ID (internal utility)
        public async Task<bool> ExistsAsync(Guid orderId)
        {
            return await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.Id == orderId);
        }

        // Get order with full details (OrderItems, Cancellations, etc.) for details page (Admin and Customer)
        public async Task<Order?> GetByIdAsync(Guid orderId)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.OrderCancellations)
                    .ThenInclude(c => c.CancellationItems)
                .Include(o => o.OrderReturns)
                    .ThenInclude(r => r.ReturnItems)
                .Include(o => o.CancellationPolicy)
                .Include(o => o.ReturnPolicy)
                .Include(o => o.Refunds)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetOrdersWithFiltersAsync(
                OrderStatusEnum? status = null,
                DateTime? fromDate = null,
                DateTime? toDate = null,
                string? searchOrderNumber = null,
                int pageNumber = 1,
                int pageSize = 20)
        {
            var query = _dbContext.Orders.AsNoTracking().AsQueryable();

            if (status.HasValue)
                query = query.Where(o => o.OrderStatusId == (int)status.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(searchOrderNumber))
            {
                searchOrderNumber = searchOrderNumber.Trim();
                query = query.Where(o => EF.Functions.Like(o.OrderNumber, $"%{searchOrderNumber}%"));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
