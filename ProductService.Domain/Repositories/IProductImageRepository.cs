using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface IProductImageRepository
    {
        Task<List<ProductImage>> GetProductImagesAsync(Guid productId);
        Task<ProductImage?> AddProductImageAsync(ProductImage image);
        Task<ProductImage?> UpdateProductImageAsync(ProductImage image);
        Task RemoveProductImageAsync(Guid imageId);
        Task<ProductImage?> GetProductImageByIdAsync(Guid imageId);
    }
}
