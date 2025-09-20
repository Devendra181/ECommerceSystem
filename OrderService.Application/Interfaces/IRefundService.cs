using OrderService.Application.DTOs.Refunds;
using OrderService.Application.DTOs.Common;
using OrderService.Domain.Enums;

namespace OrderService.Application.Interfaces
{
    public interface IRefundService
    {
        Task<RefundResponseDTO?> GetRefundByIdAsync(Guid refundId);
        Task<UpdateRefundStatusResponseDTO> UpdateRefundStatusAsync(UpdateRefundStatusRequestDTO request);
        Task DeleteRefundAsync(Guid refundId);
        Task<RefundResponseDTO?> GetByCancellationIdAsync(Guid cancellationId);
        Task<RefundResponseDTO?> GetByReturnIdAsync(Guid returnId);
        Task<PaginatedResultDTO<RefundResponseDTO>> GetRefundsByOrderIdAsync(Guid orderId, int pageNumber = 1, int pageSize = 20);
        Task<PaginatedResultDTO<RefundResponseDTO>> GetRefundsAsync(RefundFilterRequestDTO filter);
    }
}
