using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class CancellationItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CancellationId { get; set; }
        public Cancellation Cancellation { get; set; } = null!;

        public Guid OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CancellationCharge { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }
    }
}
