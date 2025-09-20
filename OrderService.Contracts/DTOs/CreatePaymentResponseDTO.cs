using OrderService.Contracts.Enums;

namespace OrderService.Contracts.DTOs
{
    public class CreatePaymentResponseDTO
    {
        public Guid PaymentId { get; set; }
        public PaymentStatusEnum Status { get; set; }
        public string? PaymentUrl { get; set; } // For online payments
        public string? ErrorMessage { get; set; }
    }
}
