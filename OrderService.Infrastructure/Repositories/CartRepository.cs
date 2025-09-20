using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly OrderDbContext _dbContext;

        public CartRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Gets the active cart for the specified user (including cart items, read-only)
        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Carts
                .Include(c => c.CartItems)    // Eager load the cart items
                .AsNoTracking()               // No tracking improves performance if no updates needed
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // Adds a new cart entity for the user
        public async Task<Cart?> AddAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            await _dbContext.Carts.AddAsync(cart); // Add new cart to EF context
            await _dbContext.SaveChangesAsync();   // Persist changes to DB
            return cart;                           // Return newly added cart with generated Id
        }

        // Updates the entire cart including its items
        public async Task<Cart?> UpdateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            _dbContext.Carts.Update(cart);          // Mark cart as modified
            await _dbContext.SaveChangesAsync();    // Persist changes to DB
            return cart;
        }

        // Deletes the entire cart and all associated items for the user
        public async Task DeleteAsyncByUserId(Guid userId)
        {
            // Retrieve cart including items
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _dbContext.CartItems.RemoveRange(cart.CartItems);  // Delete all cart items
                _dbContext.Carts.Remove(cart);                     // Delete the cart itself
                await _dbContext.SaveChangesAsync();               // Commit deletions
            }
        }

        // Removes a specific product from the user's cart by ProductId
        public async Task RemoveProductFromCartAsync(Guid userId, Guid productId)
        {
            // Load user's cart with items
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return; // Nothing to remove if no cart

            // Find the cart item for the product
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                _dbContext.CartItems.Remove(item);           // Remove the item
                await _dbContext.SaveChangesAsync();         // Save changes
            }
        }

        // Clears all items from the user's cart but keeps the cart entity
        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems.Any())
            {
                _dbContext.CartItems.RemoveRange(cart.CartItems); // Remove all items
                await _dbContext.SaveChangesAsync();
            }
        }

        // Checks if a cart exists for the given user
        public async Task<bool> CartExistsAsync(Guid userId)
        {
            return await _dbContext.Carts.AnyAsync(c => c.UserId == userId);
        }

        // Returns all cart items for the user's active cart (read-only)
        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart;
        }


        // Adds a single new cart item to the database
        public async Task<CartItem?> AddCartItemAsync(CartItem cartItem)
        {
            if (cartItem == null)
                throw new ArgumentNullException(nameof(cartItem));

            await _dbContext.CartItems.AddAsync(cartItem);   // Add new item
            await _dbContext.SaveChangesAsync();             // Commit
            return cartItem;
        }

        // Updates an existing cart item in the database
        public async Task<CartItem?> UpdateCartItemAsync(CartItem cartItem)
        {
            if (cartItem == null)
                throw new ArgumentNullException(nameof(cartItem));

            _dbContext.CartItems.Update(cartItem);            // Mark as modified
            await _dbContext.SaveChangesAsync();              // Commit
            return cartItem;
        }

        // Removes a single cart item by its CartItemId
        public async Task RemoveCartItemAsync(Guid cartItemId)
        {
            var item = await _dbContext.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _dbContext.CartItems.Remove(item);             // Remove item
                await _dbContext.SaveChangesAsync();           // Commit
            }
        }

        // Adds or updates a cart item based on a CartItem object
        public async Task<CartItem> AddOrUpdateCartItemAsync(Guid userId, CartItem cartItem)
        {
            if (cartItem == null)
                throw new ArgumentNullException(nameof(cartItem));

            if (cartItem.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId must be set on CartItem.", nameof(cartItem));

            if (cartItem.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(cartItem));

            // Get cart with items
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create new cart if not found
                cart = new Cart { UserId = userId };
                await _dbContext.Carts.AddAsync(cart);
                await _dbContext.SaveChangesAsync(); // Save to get Id
            }

            // Find existing cart item by ProductId
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItem.ProductId);

            if (existingItem != null)
            {
                // Update all relevant fields
                existingItem.ProductName = cartItem.ProductName;
                existingItem.Price = cartItem.Price;
                existingItem.Discount = cartItem.Discount;
                existingItem.Quantity = cartItem.Quantity;

                _dbContext.CartItems.Update(existingItem);
                await _dbContext.SaveChangesAsync();

                return existingItem;
            }
            else
            {
                // Add new item and associate with cart
                cartItem.Id = Guid.NewGuid();
                cartItem.CartId = cart.Id;
                await _dbContext.CartItems.AddAsync(cartItem);
                await _dbContext.SaveChangesAsync();

                return cartItem;
            }
        }

        // Merges the source user's cart into the target user's cart
        public async Task MergeCartsAsync(Guid targetUserId, Guid sourceUserId)
        {
            // TargetUserId: The user ID of the authenticated user who just logged in or registered.
            // SourceUserId: The guest/ anonymous ID used before login(temporary ID linked to the guest cart).

            // If the source and target are the same, no action needed
            if (targetUserId == sourceUserId)
                return;

            // Load target user's cart with items
            var targetCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == targetUserId);

            // Load source user's cart with items (usually guest or anonymous cart)
            var sourceCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == sourceUserId);

            if (sourceCart == null)
                return; // No source cart to merge

            if (targetCart == null)
            {
                // If target cart doesn't exist, simply assign source cart to target user
                sourceCart.UserId = targetUserId;
                await _dbContext.SaveChangesAsync();
                return;
            }

            // Merge each item from source cart into target cart
            foreach (var sourceItem in sourceCart.CartItems)
            {
                // Try find matching item in target cart by ProductId
                var targetItem = targetCart.CartItems.FirstOrDefault(ci => ci.ProductId == sourceItem.ProductId);
                if (targetItem != null)
                {
                    // If exists, increment quantity (business logic)
                    targetItem.Quantity += sourceItem.Quantity;
                    _dbContext.CartItems.Update(targetItem);
                }
                else
                {
                    // Otherwise, move the item to target cart by updating CartId
                    sourceItem.CartId = targetCart.Id;
                    _dbContext.CartItems.Update(sourceItem);
                }
            }

            // Save all merged changes
            await _dbContext.SaveChangesAsync();

            // Remove the source cart and its items (cleanup)
            _dbContext.CartItems.RemoveRange(sourceCart.CartItems);
            _dbContext.Carts.Remove(sourceCart);
            await _dbContext.SaveChangesAsync();
        }
    }
}

