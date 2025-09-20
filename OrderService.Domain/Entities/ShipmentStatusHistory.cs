using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class ShipmentStatusHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public int OldStatusId { get; set; }
        [ForeignKey(nameof(OldStatusId))]
        public ShipmentStatus? OldShipmentStatus { get; set; }
        public int NewStatusId { get; set; }
        [ForeignKey(nameof(NewStatusId))]
        public ShipmentStatus? NewShipmentStatus { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Remarks { get; set; }
        public string? Location { get; set; }
    }
}
