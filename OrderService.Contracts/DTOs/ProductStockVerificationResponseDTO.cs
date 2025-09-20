namespace OrderService.Contracts.DTOs
{
    public class ProductStockVerificationResponseDTO
    {
        public Guid ProductId { get; set; }
        public bool IsValidProduct { get; set; }
        public bool IsQuantityAvailable { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
