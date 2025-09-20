using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities
{
    public class Discount
    {
        [Key]
        public Guid Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; } // e.g., 10 for 10% off
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; } // Alternatively, flat amount off
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
