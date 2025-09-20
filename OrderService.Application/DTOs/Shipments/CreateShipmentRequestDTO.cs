using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Shipments
{
    public class CreateShipmentRequestDTO
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "CarrierName is required.")]
        [MaxLength(100, ErrorMessage = "CarrierName cannot exceed 100 characters.")]
        public string CarrierName { get; set; } = null!;

        [Required(ErrorMessage = "TrackingNumber is required.")]
        [MaxLength(100, ErrorMessage = "TrackingNumber cannot exceed 100 characters.")]
        public string TrackingNumber { get; set; } = null!;

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = null!;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public List<ShipmentItemRequestDTO>? ShipmentIItems { get; set; }
    }
}
