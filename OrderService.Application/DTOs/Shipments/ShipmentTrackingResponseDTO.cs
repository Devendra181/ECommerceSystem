using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentTrackingResponseDTO
    {
        public Guid ShipmentId { get; set; }
        public string CarrierName { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public ShipmentStatusEnum CurrentStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public List<ShipmentStatusHistoryDTO> StatusHistories { get; set; } = new();
    }
}
