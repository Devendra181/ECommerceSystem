using Messaging.Common.Models;

namespace Messaging.Common.Events
{
    //Raised by Orchestrator Service when Stock Reservation Failed
    //Consumed by Order and Notiofication Microservice
    public sealed class OrderCancelledEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<OrderItemLine> Items { get; set; } = new();
        public string Reason { get; set; } = "Stock reservation failed";
    }
}
