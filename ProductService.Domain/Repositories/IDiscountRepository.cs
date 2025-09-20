using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface IDiscountRepository
    {
        Task<List<Discount>> GetDiscountsByProductIdAsync(Guid productId);
        Task<Discount?> GetActiveDiscountByProductIdAsync(Guid productId, DateTime currentDate);
        Task<Discount?> AddDiscountAsync(Discount discount);
        Task<Discount?> UpdateDiscountAsync(Discount discount);
        Task DeleteDiscountAsync(Guid discountId);
    }
}
