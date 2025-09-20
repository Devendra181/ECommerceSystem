namespace PaymentService.Application.DTOs
{
    // DTO to return after initiating a payment
    public class PaymentInitiateResponseDTO
    {
        public Guid PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string Status { get; set; } = null!;
    }
}
