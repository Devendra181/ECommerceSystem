namespace PaymentService.Application.DTOs
{
    public class RefundGatewayResponseDTO
    {
        public string RefundId { get; set; } = null!;
        public string TransactionId { get; set; } = null!;
        public string Status { get; set; } = null!; // e.g., Success, Pending, Failed, Cancelled
        public string StatusCode { get; set; } = null!; // e.g., 200, 102, 500, 400
        public string Message { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
    }
}
