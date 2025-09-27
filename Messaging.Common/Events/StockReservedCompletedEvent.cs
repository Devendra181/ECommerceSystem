using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    //Raised by Product Microservice when Stock Reservation Completed
    //Consumed by Orchestrator Microservice, then Orchestrator service Raise OrderConfirmedEvent
    public sealed class StockReservedCompletedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public List<OrderItemLine> Items { get; set; } = new();
    }
}
