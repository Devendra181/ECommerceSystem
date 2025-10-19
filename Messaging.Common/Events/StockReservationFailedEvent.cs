using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    // Represents an event raised by the Product Microservice when
    // stock reservation for an order fails — either partially or completely.

    // Purpose:
    //   - Notifies the Orchestrator Microservice that one or more items
    //     in the order could not be reserved (e.g., out of stock, discontinued, etc.).
    //   - Triggers the Orchestrator to start the **compensation flow** by
    //     publishing an OrderCancelledEvent.

    // Flow:
    //   1️. OrchestratorService → Publishes StockReservationRequestedEvent.
    //   2️. ProductService → Consumes that event and tries to reserve stock.
    //   3️. If any item cannot be reserved → Publishes StockReservationFailedEvent.
    //   4️. OrchestratorService → Consumes this event and publishes OrderCancelledEvent.

    // Published By:
    //   - ProductService

    // Consumed By:
    //   - OrchestratorService
    public sealed class StockReservationFailedEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Order & User Information
        // --------------------------------------------------------------------
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        // --------------------------------------------------------------------
        // Failure Information
        // --------------------------------------------------------------------
        // A short, high-level reason describing why stock reservation failed.
        // Examples:
        //   - "Insufficient stock"
        //   - "Product discontinued"
        //   - "Inventory service timeout"

        // Used mainly for logs, alerts, or customer-facing notifications.
        public string Reason { get; set; } = "Insufficient stock";

        // --------------------------------------------------------------------
        // Failed Items Details
        // --------------------------------------------------------------------
        // Detailed information about which specific items in the order failed
        // and the reason for each failure.

        // Each FailedLineItem entry typically contains:
        //   - ProductId → Identifies the product that could not be reserved.
        //   - Requested → Number of units the order attempted to reserve.
        //   - Available → Number of units that were actually available at the time.
        //   - Reason → Human-readable explanation (e.g., "Only 1 left in stock").

        // This allows the OrchestratorService or NotificationService to
        // clearly communicate item-level failure reasons to users or logs.
        public List<FailedLineItem> FailedItems { get; set; } = new();
    }
}
