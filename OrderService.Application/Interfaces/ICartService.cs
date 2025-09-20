using OrderService.Application.DTOs.Cart;
namespace OrderService.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDTO?> AddItemToCartAsync(AddCartItemRequestDTO request);
        Task<CartResponseDTO?> UpdateCartItemAsync(UpdateCartItemRequestDTO request);
        Task<CartResponseDTO?> RemoveCartItemAsync(RemoveCartItemRequestDTO request);
        Task ClearCartAsync(ClearCartRequestDTO request);
        Task<CartResponseDTO?> GetCartItemsAsync(Guid? userId);
        Task<CartResponseDTO?> MergeCartsAsync(Guid targetUserId, Guid sourceUserId);
    }
}
