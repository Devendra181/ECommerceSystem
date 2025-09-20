namespace OrderService.Application.DTOs.Returns
{
    public class ReturnItemResponseDTO
    {
        public Guid ReturnItemId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
    }
}
