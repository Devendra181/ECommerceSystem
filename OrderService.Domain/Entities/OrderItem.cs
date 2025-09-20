using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public Guid ProductId { get; set; }

        [Required, MaxLength(250)]
        public string ProductName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }
        public int ItemStatusId { get; set; }

        [ForeignKey("ItemStatusId")]
        public OrderStatus OrderItemStatus { get; set; } = null!;
        public decimal TotalPrice => DiscountedPrice * Quantity;
    }
}
