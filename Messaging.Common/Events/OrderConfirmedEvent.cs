using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    //Raised by Orchestrator Service when Stock Reservation Successed
    //Consumed by Order and Notification Microservice
    public sealed class OrderConfirmedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<OrderItemLine> Items { get; set; } = new();
    }
}
