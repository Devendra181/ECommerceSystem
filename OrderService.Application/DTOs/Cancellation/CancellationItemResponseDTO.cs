namespace OrderService.Application.DTOs.Cancellation
{
    public class CancellationItemResponseDTO
    {
        public Guid CancellationItemId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
    }
}
