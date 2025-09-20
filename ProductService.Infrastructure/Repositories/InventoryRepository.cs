using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ProductDbContext _dbContext;
        public InventoryRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsStockAvailableAsync(Guid productId, int quantity)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return false;
            return product.StockQuantity >= quantity;
        }

        public async Task UpdateStockAsync(Guid productId, int quantity)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantity;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task IncreaseStockBulkAsync(IEnumerable<(Guid productId, int quantity)> stockUpdates)
        {
            foreach (var (productId, quantity) in stockUpdates)
            {
                var product = await _dbContext.Products.FindAsync(productId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found.");

                product.StockQuantity += quantity;
                _dbContext.Products.Update(product);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task DecreaseStockBulkAsync(IEnumerable<(Guid productId, int quantity)> stockUpdates)
        {
            foreach (var (productId, quantity) in stockUpdates)
            {
                var product = await _dbContext.Products.FindAsync(productId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found.");

                if (product.StockQuantity < quantity)
                    throw new InvalidOperationException($"Insufficient stock for product ID {productId}");

                product.StockQuantity -= quantity;
                _dbContext.Products.Update(product);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}
