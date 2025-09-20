using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface ICancellationRepository
    {
        Task<Cancellation?> GetByIdAsync(Guid cancellationId);
        Task<List<Cancellation>> GetCancellationsByOrderIdAsync(Guid orderId);
        Task<Cancellation?> AddAsync(Cancellation cancellation);
        Task<Cancellation?> UpdateAsync(Cancellation cancellation);
        Task DeleteAsync(Guid cancellationId);
        Task<List<Guid>> GetCancelledOrderItemIdsAsync(Guid orderId);
        Task ApproveAsync(Cancellation updatedCancellation, OrderStatusEnum newOrderStatus, string? processedBy, string? Remarks);
        Task RejectAsync(Guid cancellationId, string rejectedBy, string? remarks = null);
        Task<List<CancellationItem>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId);
        Task<bool> ExistsAsync(Guid cancellationId);
        Task<(List<Cancellation> Items, int TotalCount)> GetCancellationsWithFiltersAsync(
                Guid? userId = null,
                CancellationStatusEnum? status = null,
                DateTime? fromDate = null,
                DateTime? toDate = null,
                int pageNumber = 1,
                int pageSize = 20);
    }
}
