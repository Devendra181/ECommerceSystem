namespace ProductService.Application.DTOs
{
    public class ProductStockInfoRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
