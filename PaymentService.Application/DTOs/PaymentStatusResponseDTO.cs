namespace PaymentService.Application.DTOs
{
    // DTO to check payment status
    public class PaymentStatusResponseDTO
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
}
