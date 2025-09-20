using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IMapper _mapper;

        public ProductImageService(IProductImageRepository productImageRepository, IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductImageDTO>> GetByProductIdAsync(Guid productId)
        {
            var images = await _productImageRepository.GetProductImagesAsync(productId);
            return _mapper.Map<List<ProductImageDTO>>(images);
        }

        public async Task<ProductImageDTO?> GetByIdAsync(Guid imageId)
        {
            var image = await _productImageRepository.GetProductImageByIdAsync(imageId);
            return image == null ? null : _mapper.Map<ProductImageDTO>(image);
        }

        public async Task<ProductImageDTO?> AddAsync(ProductImageCreateDTO createDto)
        {
            var image = _mapper.Map<ProductImage>(createDto);
            image.Id = Guid.NewGuid();
            image.CreatedOn = DateTime.UtcNow;
            image.ModifiedOn = null;

            var createdImage = await _productImageRepository.AddProductImageAsync(image);
            return createdImage == null ? null : _mapper.Map<ProductImageDTO>(createdImage);
        }

        public async Task<ProductImageDTO?> UpdateAsync(ProductImageUpdateDTO updateDto)
        {
            var existingImage = await _productImageRepository.GetProductImageByIdAsync(updateDto.Id);
            if (existingImage == null) return null;

            var image = _mapper.Map<ProductImage>(updateDto);
            image.CreatedOn = existingImage.CreatedOn;
            image.ModifiedOn = DateTime.UtcNow;

            var updatedImage = await _productImageRepository.UpdateProductImageAsync(image);
            return updatedImage == null ? null : _mapper.Map<ProductImageDTO>(updatedImage);
        }

        public async Task DeleteAsync(Guid imageId)
        {
            await _productImageRepository.RemoveProductImageAsync(imageId);
        }
    }
}
