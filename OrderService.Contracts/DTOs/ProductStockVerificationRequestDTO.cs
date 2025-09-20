namespace OrderService.Contracts.DTOs
{
    public class ProductStockVerificationRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
