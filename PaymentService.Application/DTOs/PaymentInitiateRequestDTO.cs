using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    // DTO to initiate a payment request
    public class PaymentInitiateRequestDTO
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public PaymentMethodTypeEnum? PaymentMethodTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }
}