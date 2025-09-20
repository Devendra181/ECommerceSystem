using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _dbContext;
        public ProductRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            return await _dbContext.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary))
                .Include(p => p.Discounts)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<List<Product>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Discounts)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary))
                .Include(p => p.Reviews)
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchAsync(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary))
                .Include(p => p.Discounts)
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.SKU.Contains(searchTerm));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            return await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product?> AddAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Guid productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetByIdsAsync(IEnumerable<Guid> productIds)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary))
                .Include(p => p.Discounts)
                .Include(p => p.Reviews)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}
