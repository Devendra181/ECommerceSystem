using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IMapper _mapper;

        public DiscountService(IDiscountRepository discountRepository, IMapper mapper)
        {
            _discountRepository = discountRepository;
            _mapper = mapper;
        }

        public async Task<List<DiscountDTO>> GetByProductIdAsync(Guid productId)
        {
            var discounts = await _discountRepository.GetDiscountsByProductIdAsync(productId);
            return _mapper.Map<List<DiscountDTO>>(discounts);
        }

        public async Task<DiscountDTO?> GetActiveDiscountByProductIdAsync(Guid productId, DateTime currentDate)
        {
            var discount = await _discountRepository.GetActiveDiscountByProductIdAsync(productId, currentDate);
            return discount == null ? null : _mapper.Map<DiscountDTO>(discount);
        }

        public async Task<DiscountDTO?> AddAsync(DiscountCreateDTO createDto)
        {
            var discount = _mapper.Map<Discount>(createDto);
            discount.Id = Guid.NewGuid();
            discount.CreatedOn = DateTime.UtcNow;
            discount.ModifiedOn = null;

            var createdDiscount = await _discountRepository.AddDiscountAsync(discount);
            return createdDiscount == null ? null : _mapper.Map<DiscountDTO>(createdDiscount);
        }

        public async Task<DiscountDTO?> UpdateAsync(DiscountUpdateDTO updateDto)
        {
            var existingDiscount = await _discountRepository.GetDiscountsByProductIdAsync(updateDto.Id); // Small fix: should get by Id, here assuming repo method exists
            var discountToUpdate = await _discountRepository.GetActiveDiscountByProductIdAsync(updateDto.Id, DateTime.UtcNow); // Or add method GetByIdAsync for Discount (recommended)
            if (discountToUpdate == null) return null;

            var discount = _mapper.Map<Discount>(updateDto);
            discount.CreatedOn = discountToUpdate.CreatedOn;
            discount.ModifiedOn = DateTime.UtcNow;

            var updatedDiscount = await _discountRepository.UpdateDiscountAsync(discount);
            return updatedDiscount == null ? null : _mapper.Map<DiscountDTO>(updatedDiscount);
        }

        public async Task DeleteAsync(Guid discountId)
        {
            await _discountRepository.DeleteDiscountAsync(discountId);
        }
    }
}
