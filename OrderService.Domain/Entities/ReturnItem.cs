using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class ReturnItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ReturnId { get; set; }
        public Return Return { get; set; } = null!;

        public Guid OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RestockingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
