namespace PaymentService.Application.DTOs
{
    public class CheckoutSessionDTO
    {
        public string CheckoutUrl { get; set; } = default!;
        public string ProviderOrderId { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
}
