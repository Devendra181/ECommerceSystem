using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class RefundStatus
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public RefundStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
