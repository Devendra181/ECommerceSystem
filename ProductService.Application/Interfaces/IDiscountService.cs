using ProductService.Application.DTOs;
namespace ProductService.Application.Interfaces
{
    public interface IDiscountService
    {
        Task<List<DiscountDTO>> GetByProductIdAsync(Guid productId);
        Task<DiscountDTO?> GetActiveDiscountByProductIdAsync(Guid productId, DateTime currentDate);
        Task<DiscountDTO?> AddAsync(DiscountCreateDTO createDto);
        Task<DiscountDTO?> UpdateAsync(DiscountUpdateDTO updateDto);
        Task DeleteAsync(Guid discountId);
    }
}
