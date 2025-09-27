using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    //Raised by Product Microservice when Stock Reservation Failed
    //Consumed by Orchestrator Microservice, then Orchestrator service Raise OrderCancelledEvent
    public sealed class StockReservationFailedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        // High-level reason to display/log (e.g., "Insufficient stock")
        public string Reason { get; set; } = "Insufficient stock";

        // Per-line details describing which items failed and why.
        public List<FailedLine> FailedItems { get; set; } = new();
    }
}

