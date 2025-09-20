using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            return category == null ? null : _mapper.Map<CategoryDTO>(category);
        }

        public async Task<List<CategoryDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> AddAsync(CategoryCreateDTO createDto)
        {
            var category = _mapper.Map<Category>(createDto);
            category.Id = Guid.NewGuid();
            category.CreatedOn = DateTime.UtcNow;
            category.ModifiedOn = null;

            var createdCategory = await _categoryRepository.AddCategoryAsync(category);
            return createdCategory == null ? null : _mapper.Map<CategoryDTO>(createdCategory);
        }

        public async Task<CategoryDTO?> UpdateAsync(CategoryUpdateDTO updateDto)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(updateDto.Id);
            if (existingCategory == null) return null;

            var category = _mapper.Map<Category>(updateDto);
            category.CreatedOn = existingCategory.CreatedOn;
            category.ModifiedOn = DateTime.UtcNow;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
            return updatedCategory == null ? null : _mapper.Map<CategoryDTO>(updatedCategory);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
        }
    }
}
