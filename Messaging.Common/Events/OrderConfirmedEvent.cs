using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    // Represents an event raised by the Orchestrator Microservice
    // when stock reservation succeeds for an order.

    // This event is generated in response to the StockReservedCompletedEvent
    // raised by the Product Microservice.

    // Purpose:
    //   - Confirms that all products in the order have been successfully reserved.
    //   - Signals downstream microservices to proceed with post-reservation actions.

    // Consumed By:
    //   - Order Microservice → Marks the order as "Confirmed" in the database.
    //   - Notification Microservice → Sends "Order Confirmation" email/SMS to the customer.

    // Flow:
    //   1️. ProductService → Publishes StockReservedCompletedEvent (success).
    //   2️. OrchestratorService → Consumes it and raises OrderConfirmedEvent.
    //   3️. OrderService & NotificationService → Consume this event for their respective actions.
    public sealed class OrderConfirmedEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Order Details
        // --------------------------------------------------------------------
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;

        // --------------------------------------------------------------------
        // Customer Details
        // --------------------------------------------------------------------
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // --------------------------------------------------------------------
        // Order Amount
        // --------------------------------------------------------------------
        public decimal TotalAmount { get; set; }

        // --------------------------------------------------------------------
        // Ordered Products
        // --------------------------------------------------------------------
        // List of all products included in the confirmed order.

        // Each OrderLineItem contains:
        //   - ProductId → The ID of the reserved product.
        //   - Quantity → Number of units ordered.
        //   - UnitPrice → The price per unit when the order was placed.

        // This ensures every consumer (Order, Notification, Payment, etc.)
        // has a consistent view of the confirmed order contents.
        public List<OrderLineItem> Items { get; set; } = new();
    }
}
