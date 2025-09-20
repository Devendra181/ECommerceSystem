namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentItemResponseDTO
    {
        public Guid ShipmentItemId { get; set; }
        public Guid OrderItemId { get; set; }
        public int QuantityShipped { get; set; }
    }
}
