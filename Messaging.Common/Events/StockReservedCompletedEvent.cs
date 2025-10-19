using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    // Represents an event raised by the Product Microservice
    // when stock reservation is successfully completed for an order.

    // Purpose:
    //   - Indicates that the inventory check and stock reservation
    //     have been successfully processed.
    //   - Serves as a "Success Signal" back to the Orchestrator Microservice
    //     to continue the Saga flow and confirm the order.

    // Flow:
    //   1️. OrchestratorService → Publishes StockReservationRequestedEvent.
    //   2️. ProductService → Consumes that request and checks inventory.
    //   3️. If all items have sufficient stock, ProductService → Publishes StockReservedCompletedEvent.
    //   4️. OrchestratorService → Consumes this event and publishes OrderConfirmedEvent.

    // Published By:
    //   - ProductService

    // Consumed By:
    //   - OrchestratorService
    public sealed class StockReservedCompletedEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Order and Customer Information
        // --------------------------------------------------------------------
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        // --------------------------------------------------------------------
        // Reserved Product Items
        // --------------------------------------------------------------------
        // The list of items (products) for which stock has been successfully reserved.

        // Each item in this list contains:
        //   - ProductId  → Which product was reserved.
        //   - Quantity   → How many units were locked.
        //   - UnitPrice  → The price at which it was ordered (for context, not validation).

        // The OrchestratorService includes these items when raising
        // the next event (OrderConfirmedEvent), ensuring all services
        // share a consistent view of what was reserved.
        public List<OrderLineItem> Items { get; set; } = new();
    }
}
