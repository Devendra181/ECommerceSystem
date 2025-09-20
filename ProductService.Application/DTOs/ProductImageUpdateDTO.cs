using System.ComponentModel.DataAnnotations;
namespace ProductService.Application.DTOs
{
    // For Update - with Id
    public class ProductImageUpdateDTO
    {
        [Required(ErrorMessage = "Id is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string ImageUrl { get; set; } = null!;

        public bool IsPrimary { get; set; }

        [StringLength(500, ErrorMessage = "Alternative Text cannot exceed 500 characters.")]
        public string? AltText { get; set; }
    }
}
