namespace OrderService.Application.DTOs.Cart
{
    public class CartItemResponseDTO
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => (UnitPrice - Discount) * Quantity;
    }
}

