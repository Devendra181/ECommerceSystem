using OrderService.Contracts.Enums;

namespace OrderService.Contracts.DTOs
{
    // Used by Order Microservice to create payment record via Payment Microservice
    public class CreatePaymentRequestDTO
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
