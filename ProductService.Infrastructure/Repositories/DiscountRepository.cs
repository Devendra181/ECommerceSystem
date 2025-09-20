using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly ProductDbContext _dbContext;
        public DiscountRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Discount>> GetDiscountsByProductIdAsync(Guid productId)
        {
            return await _dbContext.Discounts
                .Where(d => d.ProductId == productId)
                .ToListAsync();
        }

        public async Task<Discount?> GetActiveDiscountByProductIdAsync(Guid productId, DateTime currentDate)
        {
            return await _dbContext.Discounts
                .Where(d => d.ProductId == productId && d.IsActive && d.StartDate <= currentDate && d.EndDate >= currentDate)
                .OrderByDescending(d => d.DiscountPercentage)
                .FirstOrDefaultAsync();
        }

        public async Task<Discount?> AddDiscountAsync(Discount discount)
        {
            await _dbContext.Discounts.AddAsync(discount);
            await _dbContext.SaveChangesAsync();
            return discount;
        }

        public async Task<Discount?> UpdateDiscountAsync(Discount discount)
        {
            _dbContext.Discounts.Update(discount);
            await _dbContext.SaveChangesAsync();
            return discount;
        }

        public async Task DeleteDiscountAsync(Guid discountId)
        {
            var discount = await _dbContext.Discounts.FindAsync(discountId);
            if (discount != null)
            {
                _dbContext.Discounts.Remove(discount);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
