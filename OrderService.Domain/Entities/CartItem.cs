using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CartId { get; set; }
        public Cart? Cart { get; set; }

        public Guid ProductId { get; set; }

        [Required, MaxLength(250)]
        public string ProductName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }
    }
}
