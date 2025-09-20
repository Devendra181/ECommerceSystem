using ProductService.Application.DTOs;
namespace ProductService.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO?> GetByIdAsync(Guid id);
        Task<List<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO?> AddAsync(CategoryCreateDTO createDto);
        Task<CategoryDTO?> UpdateAsync(CategoryUpdateDTO updateDto);
        Task DeleteAsync(Guid id);
    }
}
