using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class ReturnStatus
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public ReturnStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
