using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    //Raised by Orchestrator Microservice when Stock Reservation Requested
    //Consumed by Product Microservice
    // - Based on the Outcome the Product Microservice Raise
    //     1. Sucess: StockReservedCompletedEvent
    //     2. Failed: StockReservationFailedEvent
    public sealed class StockReservationRequestedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public List<OrderItemLine> Items { get; set; } = new();
    }
}

