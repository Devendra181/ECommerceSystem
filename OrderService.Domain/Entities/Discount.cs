using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        // New Year 10% Off, Flat $50 Off, Summer, Winter
        [Required, MaxLength(150)]
        public string DiscountName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Type of discount (e.g., Percentage, FixedAmount)
        [Required, MaxLength(50)]
        public DiscountTypeEnum DiscountType { get; set; }

        // Percentage value for percentage discount (e.g., 10 for 10%)
        // Nullable if discount is fixed amount.
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        // Fixed amount discount value
        // Nullable if discount is percentage.
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
