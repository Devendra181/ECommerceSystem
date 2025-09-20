using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ProductDTO?> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return null;

            var productDto = _mapper.Map<ProductDTO>(product);

            // Since filtered Include loads only primary images, pick first if exists
            var primaryImage = product.ProductImages.FirstOrDefault();
            productDto.PrimaryImageUrl = primaryImage?.ImageUrl;
            productDto.PrimaryImageAltText = primaryImage?.AltText;

            return productDto;
        }

        public async Task<List<ProductDTO>> GetAllAsync(int pageNumber, int pageSize)
        {
            var products = await _productRepository.GetAllAsync(pageNumber, pageSize);

            var productDTOs = _mapper.Map<List<ProductDTO>>(products);

            for (int i = 0; i < products.Count; i++)
            {
                var primaryImage = products[i].ProductImages.FirstOrDefault(pi => pi.IsPrimary);
                productDTOs[i].PrimaryImageUrl = primaryImage?.ImageUrl;
                productDTOs[i].PrimaryImageAltText = primaryImage?.AltText;
            }

            return productDTOs;
        }

        public async Task<List<ProductDTO>> SearchAsync(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            var products = await _productRepository.SearchAsync(searchTerm, categoryId, minPrice, maxPrice, pageNumber, pageSize);
            var productDTOs =  _mapper.Map<List<ProductDTO>>(products);

            for (int i = 0; i < products.Count; i++)
            {
                var primaryImage = products[i].ProductImages.FirstOrDefault(pi => pi.IsPrimary);
                productDTOs[i].PrimaryImageUrl = primaryImage?.ImageUrl;
                productDTOs[i].PrimaryImageAltText = primaryImage?.AltText;
            }

            return productDTOs;
        }

        public async Task<ProductDTO?> AddAsync(ProductCreateDTO createDto)
        {
            var product = _mapper.Map<Product>(createDto);
            product.Id = Guid.NewGuid();
            product.CreatedOn = DateTime.UtcNow;
            product.ModifiedOn = null;
            // Generate SKU dynamically before saving
            product.SKU = await GenerateProductSKUAsync(product);

            var createdProduct = await _productRepository.AddAsync(product);
            return createdProduct == null ? null : _mapper.Map<ProductDTO>(createdProduct);
        }

        public async Task<ProductDTO?> UpdateAsync(ProductUpdateDTO updateDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(updateDto.Id);
            if (existingProduct == null) 
                return null;

            var product = _mapper.Map<Product>(updateDto);
            product.CreatedOn = existingProduct.CreatedOn; // keep original created time
            product.ModifiedOn = DateTime.UtcNow;
            // Generate SKU dynamically before saving
            product.SKU = await GenerateProductSKUAsync(product);

            var updatedProduct = await _productRepository.UpdateAsync(product);
            return updatedProduct == null ? null : _mapper.Map<ProductDTO>(updatedProduct);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
        }

        private async Task<string> GenerateProductSKUAsync(Product product)
        {
            // Get category by Id to fetch Category Name
            var category = await _categoryRepository.GetCategoryByIdAsync(product.CategoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            string categoryPart = GetFirst3Letters(category.Name);
            string productPart = GetFirst3Letters(product.Name);
            string yearPart = DateTime.UtcNow.Year.ToString();

            // Get last 4 chars of Product GUID (without hyphens, uppercase)
            string guidSuffix = product.Id.ToString("N").Substring(28, 4).ToUpper();

            // Combine parts to SKU
            return $"{categoryPart}-{productPart}-{yearPart}-{guidSuffix}";
        }

        public async Task<List<ProductDTO>> GetByIdsAsync(IEnumerable<Guid> productIds)
        {
            var products = await _productRepository.GetByIdsAsync(productIds);
            return _mapper.Map<List<ProductDTO>>(products);
        }

        private string GetFirst3Letters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "XXX"; // fallback

            return new string(input.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper().PadRight(3, 'X');
        }
    }
}
