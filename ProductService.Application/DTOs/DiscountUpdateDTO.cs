using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    // For Update (with Id)
    public class DiscountUpdateDTO
    {
        [Required(ErrorMessage = "Discount Id is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Discount Name is required.")]
        [StringLength(100, ErrorMessage = "Discount Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Percentage must be between 0 and 100.")]
        public decimal? DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount Amount must be a positive value.")]
        public decimal? DiscountAmount { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
