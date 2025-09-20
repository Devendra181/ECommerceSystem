using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain.Entities
{
    public class ShipmentStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ShipmentStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
