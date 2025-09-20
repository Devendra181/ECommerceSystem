using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ProductDbContext _dbContext;
        public ProductImageRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProductImage>> GetProductImagesAsync(Guid productId)
        {
            return await _dbContext.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetProductImageByIdAsync(Guid imageId)
        {
            return await _dbContext.ProductImages.FindAsync(imageId);
        }

        public async Task<ProductImage?> AddProductImageAsync(ProductImage image)
        {
            await _dbContext.ProductImages.AddAsync(image);
            await _dbContext.SaveChangesAsync();
            return image;
        }

        public async Task<ProductImage?> UpdateProductImageAsync(ProductImage image)
        {
            _dbContext.ProductImages.Update(image);
            await _dbContext.SaveChangesAsync();
            return image;
        }

        public async Task RemoveProductImageAsync(Guid imageId)
        {
            var image = await _dbContext.ProductImages.FindAsync(imageId);
            if (image != null)
            {
                _dbContext.ProductImages.Remove(image);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
