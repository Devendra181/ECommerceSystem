using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentStatusUpdateRequestDTO
    {
        [Required(ErrorMessage = "ShipmentId is required.")]
        public Guid ShipmentId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public ShipmentStatusEnum Status { get; set; }

        [MaxLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public string? Location { get; set; }
        public string? ChangedBy { get; set; }
    }
}
