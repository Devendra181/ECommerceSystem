using ProductService.Application.DTOs;
namespace ProductService.Application.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDTO>> GetByProductIdAsync(Guid productId);
        Task<ReviewDTO?> GetByIdAsync(Guid reviewId);
        Task<ReviewDTO?> AddAsync(ReviewCreateDTO createDto);
        Task<ReviewDTO?> UpdateAsync(ReviewUpdateDTO updateDto);
        Task DeleteAsync(Guid reviewId);
    }
}
