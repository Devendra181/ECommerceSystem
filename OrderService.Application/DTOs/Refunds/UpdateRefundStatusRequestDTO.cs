using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Refunds
{
    public class UpdateRefundStatusRequestDTO
    {
        [Required]
        public Guid RefundId { get; set; }
        [Required]
        public RefundStatusEnum NewStatus { get; set; }
        [Required]
        public string ProcessedBy { get; set; } = null!;
        public string? Remarks { get; set; }
    }
}
