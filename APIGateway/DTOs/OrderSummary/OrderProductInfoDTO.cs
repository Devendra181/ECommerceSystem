namespace APIGateway.DTOs.OrderSummary
{
    public class OrderProductInfoDTO
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
