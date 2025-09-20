using OrderService.Contracts.Enums;

namespace OrderService.Contracts.DTOs
{
    public class PaymentInfoResponseDTO
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionReference { get; set; }
        public string? FailureReason { get; set; }
    }
}
