using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain.Entities
{
    public class ShipmentItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public int QuantityShipped { get; set; }
    }
}
