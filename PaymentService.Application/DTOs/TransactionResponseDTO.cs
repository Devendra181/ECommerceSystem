namespace PaymentService.Application.DTOs
{
    public class TransactionResponseDTO
    {
        public Guid TransactionId { get; set; }
        public string TransactionStatus { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Message { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? GatewayTransactionId { get; set; } = null!;
    }
}
