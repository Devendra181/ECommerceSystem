using OrderService.Application.DTOs.Cancellation;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Returns;

namespace OrderService.Application.Interfaces
{
    public interface IReturnService
    {
        Task<ReturnResponseDTO> CreateReturnRequestAsync(CreateReturnRequestDTO request);
        Task<ReturnResponseDTO?> GetReturnByIdAsync(Guid returnId);
        Task<List<ReturnResponseDTO>?> GetReturnsByOrderIdAsync(Guid orderId);
        Task<ReturnResponseDTO?> UpdateReturnAsync(UpdateReturnRequestDTO request);
        Task DeleteReturnAsync(Guid cancellationId);
        Task<List<ReturnItemResponseDTO>> GetReturnItemsByReturnIdAsync(Guid returnId);
        Task<PaginatedResultDTO<ReturnResponseDTO>> GetReturnByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<PaginatedResultDTO<ReturnResponseDTO>> GetReturnAsync(ReturnFilterRequestDTO filter);
        Task<ReturnApprovalResponseDTO> ApproveOrRejectReturnAsync(ReturnApprovalRequestDTO request, string accessToken);
    }
}
