using OrderService.Domain.Enums;
namespace OrderService.Application.DTOs.Order
{
    public class OrderItemResponseDTO
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public OrderStatusEnum ItemStatusId { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}