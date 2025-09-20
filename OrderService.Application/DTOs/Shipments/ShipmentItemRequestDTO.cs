using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Shipments
{
    public class ShipmentItemRequestDTO
    {
        [Required(ErrorMessage = "OrderItemId is required.")]
        public Guid OrderItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "QuantityShipped must be at least 1.")]
        public int QuantityShipped { get; set; }
    }
}
