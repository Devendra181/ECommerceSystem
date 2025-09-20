using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid productId);
        Task<List<Product>> GetAllAsync(int pageNumber, int pageSize);
        Task<List<Product>> SearchAsync(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<Product?> AddAsync(Product product);
        Task<Product?> UpdateAsync(Product product);
        Task DeleteAsync(Guid productId);
        Task<List<Product>> GetByIdsAsync(IEnumerable<Guid> productIds);
    }
}
