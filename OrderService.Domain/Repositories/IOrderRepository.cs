using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> AddAsync(Order order);
        Task<Order?> GetByIdAsync(Guid orderId);
        Task<List<Order>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> ChangeOrderStatusAsync(Guid orderId, OrderStatusEnum newStatusId, string? changedBy = null, string? remarks = null);
        Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(Guid orderId);
        Task DeleteAsync(Guid orderId);
        Task<bool> ExistsAsync(Guid orderId);

        Task<(List<Order> Items, int TotalCount)> GetOrdersWithFiltersAsync(
                OrderStatusEnum? status = null,
                DateTime? fromDate = null,
                DateTime? toDate = null,
                string? searchOrderNumber = null,
                int pageNumber = 1,
                int pageSize = 20);
    }
}
