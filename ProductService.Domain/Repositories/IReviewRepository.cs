using ProductService.Domain.Entities;
namespace ProductService.Domain.Repositories
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetReviewsByProductIdAsync(Guid productId);
        Task<Review?> GetReviewByIdAsync(Guid reviewId);
        Task<Review?> AddReviewAsync(Review review);
        Task<Review?> UpdateReviewAsync(Review review);
        Task DeleteReviewAsync(Guid reviewId);
    }
}
