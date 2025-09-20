using OrderService.Application.DTOs.Cancellation;
using OrderService.Application.DTOs.Common;

namespace OrderService.Application.Interfaces
{
    public interface ICancellationService
    {
        Task<CancellationResponseDTO> CreateCancellationRequestAsync(CreateCancellationRequestDTO request);
        Task<CancellationResponseDTO?> GetCancellationByIdAsync(Guid cancellationId);
        Task<List<CancellationResponseDTO>?> GetCancellationsByOrderIdAsync(Guid orderId);      
        Task<CancellationResponseDTO?> UpdateCancellationAsync(UpdateCancellationRequestDTO request);
        Task DeleteCancellationAsync(Guid cancellationId);
        Task<List<CancellationItemResponseDTO>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId);
        Task<PaginatedResultDTO<CancellationResponseDTO>> GetCancellationsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<PaginatedResultDTO<CancellationResponseDTO>> GetCancellationsAsync(CancellationFilterRequestDTO filter);
        Task<CancellationApprovalResponseDTO> ApproveOrRejectCancellationAsync(CancellationApprovalRequestDTO request, string accessToken);
    }
}
