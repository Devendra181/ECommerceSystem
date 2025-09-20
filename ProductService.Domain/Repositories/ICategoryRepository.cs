using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> AddCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid categoryId);
    }
}
