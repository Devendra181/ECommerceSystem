using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface IReturnRepository
    {
        Task<Return?> GetByIdAsync(Guid returnId);
        Task<List<Return>> GetReturnsByOrderIdAsync(Guid orderId);
        Task<Return?> AddAsync(Return returnRequest);
        Task<Return?> UpdateAsync(Return returnRequest);
        Task DeleteAsync(Guid returnId);
        Task<List<Guid>> GetReturnedOrderItemIdsAsync(Guid orderId);
        Task ApproveAsync(Return updatedReturn, string approvedBy, decimal refundableAmount, string? remarks = null);
        Task RejectAsync(Guid returnId, string rejectedBy, string? remarks = null);
        Task<List<ReturnItem>> GetReturnItemsByReturnIdAsync(Guid returnId);
        Task<bool> ExistsAsync(Guid returnId);
        Task<(List<Return> Items, int TotalCount)> GetReturnsWithFiltersAsync(
            Guid? userId = null,
            ReturnStatusEnum? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}
