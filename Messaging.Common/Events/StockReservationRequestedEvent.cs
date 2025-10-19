using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    // Represents an event raised by the Orchestrator Microservice to request
    // stock reservation from the Product Microservice.

    // Purpose:
    //   - Acts as the "next step" in the Saga flow after an order is placed.
    //   - Tells the ProductService to check inventory and reserve stock
    //     for all items in the order.

    // Flow:
    //   1️. OrderService publishes OrderPlacedEvent.
    //   2️. OrchestratorService consumes that event.
    //   3️. OrchestratorService raises StockReservationRequestedEvent → ProductService.
    //   4️. ProductService processes the request and:
    //        - If successful → raises StockReservedCompletedEvent.
    //        - If failed → raises StockReservationFailedEvent.

    // Published By:
    //   - OrchestratorService

    // Consumed By:
    //   - ProductService
    public sealed class StockReservationRequestedEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Event Metadata (Inherited from EventBase)
        // --------------------------------------------------------------------
        // EventBase provides:
        //   - EventId       → Unique ID for this event instance.
        //   - Timestamp     → Time when this event was created.
        //   - CorrelationId → Shared across all related events in the same Saga transaction.

        // This ensures that every event in the Order workflow
        // (OrderPlaced, StockRequested, StockReserved, etc.)
        // can be traced end-to-end across all microservices.

        // --------------------------------------------------------------------
        // Order and Customer Identification
        // --------------------------------------------------------------------
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        // --------------------------------------------------------------------
        // Items for Stock Reservation
        // --------------------------------------------------------------------
        // A list of all the products included in this order
        // that require stock reservation.

        // Each item is represented by an OrderLineItem model, which includes:
        //   - ProductId  → Identifies which product to reserve.
        //   - Quantity   → Number of units needed.
        //   - UnitPrice  → Price at the time of ordering (for reference or logging).

        // The ProductService will:
        //   - Check the inventory for each ProductId.
        //   - Reserves the requested Quantity if available.
        //   - Return a success or failure event accordingly.
        public List<OrderLineItem> Items { get; set; } = new();
    }
}

