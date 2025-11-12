namespace APIGateway.DTOs.OrderSummary
{
    public class PaymentInfoDTO
    {
        public Guid? PaymentId { get; set; }
        public string Status { get; set; } = "Unknown";
        public string Method { get; set; } = "Unknown";
        public DateTime? PaidOn { get; set; }
        public string? TransactionReference { get; set; }
    }
}
