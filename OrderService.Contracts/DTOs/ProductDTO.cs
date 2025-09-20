namespace OrderService.Contracts.DTOs
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal DiscountedPrice { get; set; }
        // Calculated property - no setter, computed on the fly
        public decimal Discount => Price - DiscountedPrice;
    }
}
