using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class Tax
    {
        [Key]
        public int Id { get; set; }

        //CGST, SGST, Shipping Tax

        [Required, MaxLength(150)]
        public string TaxName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Tax percentage rate (e.g., 5.0 means 5%)
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; }

        // Indicates whether tax is applied to product price or shipping or both
        public bool AppliesToProduct { get; set; }
        public bool AppliesToShipping { get; set; }
        public bool IsActive { get; set; }

        // Optional start date for tax applicability (e.g. for new tax rates)
        public DateTime? ValidFrom { get; set; }

        // Optional end date for tax applicability (for expired tax rules)
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
