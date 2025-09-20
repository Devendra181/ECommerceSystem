using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentResponseDTO
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public ShipmentStatusEnum Status { get; set; }
        public string CarrierName { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public List<ShipmentItemResponseDTO>? ShipmentItems { get; set; }
    }
}
