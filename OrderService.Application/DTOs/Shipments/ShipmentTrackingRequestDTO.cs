using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentTrackingRequestDTO
    {
        [Required(ErrorMessage = "TrackingNumber is Required")]
        public string TrackingNumber { get; set; } = null!;
    }
}
