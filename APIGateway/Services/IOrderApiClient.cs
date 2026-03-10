using APIGateway.DTOs.Common;
using OrderService.Application.DTOs.Order;
namespace APIGateway.Services
{
    public interface IOrderApiClient
    {
        // Calls OrderService to create a new order.
        // request               : The order request data (userId, items, addresses, payment method etc.)
        // authorizationHeader   : The incoming Authorization header from client request (forwarded to OrderService)
        // cancellationToken     : Supports request cancellation (timeout, client disconnect, etc.)
        Task<ApiResponse<OrderResponseDTO>> CreateOrderAsync(
            CreateOrderRequestDTO request,
            string? authorizationHeader,
            CancellationToken cancellationToken = default);
    }
}
