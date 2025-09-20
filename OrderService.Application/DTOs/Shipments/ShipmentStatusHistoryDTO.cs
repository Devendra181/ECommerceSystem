using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentStatusHistoryDTO
    {
        public Guid Id { get; set; }
        public ShipmentStatusEnum OldStatus { get; set; }
        public ShipmentStatusEnum NewStatus { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public string? Remarks { get; set; }
        public string? Location { get; set; }
    }
}
