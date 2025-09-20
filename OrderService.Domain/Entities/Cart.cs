using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
