using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(CreateOrderRequestDTO request, string accessToken);
        Task<OrderResponseDTO?> GetOrderByIdAsync(Guid orderId);
        Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<List<OrderStatusHistoryResponseDTO>> GetOrderStatusHistoryAsync(Guid orderId);
        Task<bool> ConfirmOrderAsync(Guid orderId, string accessToken);
        Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersAsync(OrderFilterRequestDTO filter);
        Task<ChangeOrderStatusResponseDTO> ChangeOrderStatusAsync(ChangeOrderStatusRequestDTO request);
    }
}
