using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart?> AddAsync(Cart cart);
        Task<Cart?> UpdateAsync(Cart cart);
        Task DeleteAsyncByUserId(Guid userId);
        Task RemoveProductFromCartAsync(Guid userId, Guid productId);
        Task ClearCartAsync(Guid userId);
        Task<bool> CartExistsAsync(Guid userId);
        Task<Cart?> GetCartByUserIdAsync(Guid userId);
        Task<CartItem?> AddCartItemAsync(CartItem cartItem);
        Task<CartItem?> UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(Guid cartItemId);
        Task<CartItem> AddOrUpdateCartItemAsync(Guid userId, CartItem cartItem);
        Task MergeCartsAsync(Guid targetUserId, Guid sourceUserId);
    }
}
