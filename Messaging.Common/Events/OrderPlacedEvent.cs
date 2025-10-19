using Messaging.Common.Models;
namespace Messaging.Common.Events
{
    // This event is published by the Order Microservice after
    // a new order is successfully created and saved to the database with Pending State

    // Purpose:
    //   - This event marks the beginning of the Saga orchestration process.
    //   - It is published to RabbitMQ so that the Orchestrator Microservice
    //     can listen and trigger the next step — stock reservation.

    // Consumers:
    //   - OrchestratorService (which then publishes StockReservationRequestedEvent)

    // Flow:
    //   1️. OrderService places an order and raises this event.
    //   2️. OrchestratorService consumes this event.
    //   3️. OrchestratorService requests ProductService to reserve stock.
    public sealed class OrderPlacedEvent : EventBase
    {
        // --------------------------------------------------------------------
        // Event Metadata (Inherited from EventBase)
        // --------------------------------------------------------------------
        // EventBase includes:
        //   - EventId: Unique ID for each event instance.
        //   - Timestamp: UTC time when the event was created.
        //   - CorrelationId: Tracks this event across multiple microservices in the same workflow.
        // Example: OrderPlacedEvent → StockReservationRequestedEvent → OrderConfirmedEvent
        // All will share the same CorrelationId for traceability.

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
        // Ordered Items
        // --------------------------------------------------------------------
        // Shared Structure:
        //   - ProductId → Identifies which product
        //   - Quantity → Number of units ordered
        //   - UnitPrice → Price per unit at the time of order
        public List<OrderLineItem> Items { get; set; } = new();
    }
}
