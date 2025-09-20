namespace PaymentService.Application.DTOs
{
    public class ManualRefundProcessResponseDTO
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = default!;
        public decimal RefundAmount { get; set; }
        public string RefundMethod { get; set; } = default!;
        public DateTime? ProcessedAt { get; set; }
        public Guid TransactionId { get; set; }
        public string SettlementReference { get; set; } = default!;
        public Guid PerformedBy { get; set; }
        public string? Notes { get; set; }
    }
}

