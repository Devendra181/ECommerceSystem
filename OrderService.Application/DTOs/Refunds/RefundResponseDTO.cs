using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Refunds
{
    public class RefundResponseDTO
    {
        public Guid RefundId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? CancellationId { get; set; }  // Refund linked to Cancellation or Return
        public Guid? ReturnId { get; set; } // Refund linked to Cancellation or Return
        public decimal RefundAmount { get; set; }
        public RefundStatusEnum RefundStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Remarks { get; set; }
    }
}
