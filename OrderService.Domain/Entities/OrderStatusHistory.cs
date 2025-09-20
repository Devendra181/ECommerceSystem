using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class OrderStatusHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [Required]
        public int OldStatusId { get; set; }

        [ForeignKey(nameof(OldStatusId))]
        public OrderStatus? OldStatus { get; set; }

        [Required]
        public int NewStatusId { get; set; }

        [ForeignKey(nameof(NewStatusId))]
        public OrderStatus? NewStatus { get; set; }

        [MaxLength(100)]
        public string? ChangedBy { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; }
    }
}
