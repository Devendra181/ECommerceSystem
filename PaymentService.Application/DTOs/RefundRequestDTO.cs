namespace PaymentService.Application.DTOs
{
    // DTO to request a refund
    public class RefundRequestDTO
    {
        public Guid PaymentId { get; set; }
        public Guid? OriginalTransactionId { get; set; }
        public decimal RefundAmount { get; set; }
        public string? Reason { get; set; }
        public Guid InitiatedBy { get; set; }
    }
}
