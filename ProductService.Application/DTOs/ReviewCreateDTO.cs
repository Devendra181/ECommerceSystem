using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    // For Create - no Id
    public class ReviewCreateDTO
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
        public string? Comment { get; set; }
    }
}
