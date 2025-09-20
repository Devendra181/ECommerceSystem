namespace PaymentService.Application.DTOs
{
    public class PaymentWebhookEventDTO
    {
        public Guid? PaymentId { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid? RefundId { get; set; }
        public string Status { get; set; } = null!; // e.g., "Success","Failed","Pending","Cancelled"
        public string StatusCode { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public string RawBody { get; set; } = null!;  // full payload for audit
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime EventTimeUtc { get; set; }
    }
}
