using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface IRefundRepository
    {
        Task<Refund> AddAsync(Refund refund);
        Task<Refund?> GetByIdAsync(Guid refundId);
        Task<Refund> UpdateStatusAsync(Guid refundId, RefundStatusEnum newStatus, string processedBy, string? remarks = null);
        Task DeleteAsync(Guid refundId);
        Task<bool> ExistsAsync(Guid refundId);
        Task<Refund?> GetByCancellationIdAsync(Guid cancellationId);
        Task<Refund?> GetByReturnIdAsync(Guid returnId);
        Task<(List<Refund> Items, int TotalCount)> GetByOrderIdAsync(Guid orderId, int pageNumber = 1, int pageSize = 20);
        Task<(List<Refund> Items, int TotalCount)> GetRefundsWithFiltersAsync(
            Guid? userId = null,
            RefundStatusEnum? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}
