using ProductService.Application.DTOs;
namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDTO?> GetByIdAsync(Guid id);
        Task<List<ProductDTO>> GetAllAsync(int pageNumber, int pageSize);
        Task<List<ProductDTO>> SearchAsync(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<ProductDTO?> AddAsync(ProductCreateDTO productDto);
        Task<ProductDTO?> UpdateAsync(ProductUpdateDTO productDto);
        Task DeleteAsync(Guid productId);
        Task<List<ProductDTO>> GetByIdsAsync(IEnumerable<Guid> productIds);
    }
}
