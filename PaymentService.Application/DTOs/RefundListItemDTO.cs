namespace PaymentService.Application.DTOs
{
    public class RefundListItemDTO
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = default!;
        public string RefundMethod { get; set; } = default!;
        public decimal RefundAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? InitiatedBy { get; set; }
    }
}
