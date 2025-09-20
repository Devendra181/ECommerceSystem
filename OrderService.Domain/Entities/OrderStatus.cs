using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public OrderStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
