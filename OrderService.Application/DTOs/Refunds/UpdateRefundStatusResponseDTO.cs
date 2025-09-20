using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Refunds
{
    public class UpdateRefundStatusResponseDTO
    {
        public Guid RefundId { get; set; }
        public RefundStatusEnum Status { get; set; }
        public string? Message { get; set; } = null!;
        public DateTime ProcessedOn { get; set; } 
    }
}
