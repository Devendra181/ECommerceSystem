namespace ProductService.Application.DTOs
{
    public class ProductStockInfoResponseDTO
    {
        public Guid ProductId { get; set; }
        public bool IsValidProduct { get; set; }
        public bool IsQuantityAvailable { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
