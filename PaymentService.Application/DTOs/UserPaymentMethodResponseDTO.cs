namespace PaymentService.Application.DTOs
{
    // DTO to represent user payment method for display
    public class UserPaymentMethodResponseDTO
    {
        public Guid PaymentMethodId { get; set; }
        public string MaskedDetails { get; set; } = null!;
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public string PaymentMethodType { get; set; } = null!;
    }
}
