using OrderService.Contracts.Enums;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Order
{
    public class OrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public OrderStatusEnum OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemResponseDTO> Items { get; set; } = new();
        public Guid ShippingAddressId { get; set; }
        public Guid BillingAddressId { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public int? CancellationPolicyId { get; set; }
        public int? ReturnPolicyId { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentUrl { get; set; }
    }
}




