using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ProductDbContext _dbContext;
        public ReviewRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            return await _dbContext.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(Guid reviewId)
        {
            return await _dbContext.Reviews.FindAsync(reviewId);
        }

        public async Task<Review?> AddReviewAsync(Review review)
        {
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task<Review?> UpdateReviewAsync(Review review)
        {
            _dbContext.Reviews.Update(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task DeleteReviewAsync(Guid reviewId)
        {
            var review = await _dbContext.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _dbContext.Reviews.Remove(review);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
