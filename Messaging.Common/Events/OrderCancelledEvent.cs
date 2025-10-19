using Messaging.Common.Models;

namespace Messaging.Common.Events
{
    // Represents an event raised by the Orchestrator Microservice when
    // stock reservation fails for an order.

    // This event is created in response to the StockReservationFailedEvent
    // raised by the Product Microservice.

    // Purpose:
    //   - Notifies downstream services that the order could not be confirmed.
    //   - Informs the Order Microservice to mark the order as "Cancelled".
    //   - Informs the Notification Microservice to send a "Cancellation Email" to the user.

    // Flow:
    //   1️. ProductService → Publishes StockReservationFailedEvent.
    //   2️. OrchestratorService → Consumes it and raises OrderCancelledEvent.
    //   3️. OrderService → Updates order status to "Cancelled".
    //   4️. NotificationService → Sends cancellation message to customer.

    // Published By:
    //   - OrchestratorService

    // Consumed By:
    //   - OrderService
    //   - NotificationService
    public sealed class OrderCancelledEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Order Information
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
        // Cancellation Reason
        // --------------------------------------------------------------------
        // A high-level description of why the order was cancelled.

        // Examples:
        //   - "Stock Reservation Failed"
        //   - "One or more items are unavailable"
        public string Reason { get; set; } = "Stock Reservation Failed";

        // --------------------------------------------------------------------
        // Failed Item Details
        // --------------------------------------------------------------------
        // Contains detailed information about which items failed during
        // the stock reservation process, leading to this order being cancelled.

        // Each FailedLineItem entry includes:
        //   - ProductId → Which product caused the failure.
        //   - Requested → Quantity originally ordered.
        //   - Available → Quantity actually available in stock.
        //   - Reason → Specific reason for failure (e.g., "Insufficient stock").

        // This list allows NotificationService to communicate exactly which
        // products failed and helps OrderService for auditing or analytics.
        public List<FailedLineItem> Items { get; set; } = new();
    }
}
