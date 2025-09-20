using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class ReasonMaster
    {
        [Key]
        public int Id { get; set; }

        // Cancellation, Return
        [Required, MaxLength(50)]
        public ReasonTypeEnum ReasonType { get; set; }

        // Cancellation Reasons
        // Ordered by mistake, Found better price, Shipping too slow

        // Return Reasons
        // Product defective, Product not as described, Wrong item delivered

        [Required, MaxLength(500)]
        public string ReasonText { get; set; } = null!;
    }
}
