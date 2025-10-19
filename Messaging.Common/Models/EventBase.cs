namespace Messaging.Common.Models
{
    // The base class for all domain events in the system.
    // Every event that travels across microservices (e.g., OrderPlacedEvent,
    // StockReservedCompletedEvent, OrderCancelledEvent) inherits from this class.

    // Purpose:
    //   - Provides consistent metadata for all events.
    //   - Enables tracking, logging, and distributed tracing across services.
    //   - Ensures each event is uniquely identifiable.
    public abstract class EventBase
    {
        // ---------------------------------------------------------------------
        // Unique Event Identifier
        // ---------------------------------------------------------------------
        // A unique GUID automatically assigned to each event when it is created.
        // This helps distinguish between multiple instances of the same event type.

        // Example:
        //   - Two separate OrderPlacedEvent messages will each have their own EventId.

        // Used for:
        //   Deduplication checks
        //   Logging and debugging
        //   Tracking messages in monitoring tools
        public Guid EventId { get; private set; } = Guid.NewGuid();

        // ---------------------------------------------------------------------
        // Event Creation Timestamp
        // ---------------------------------------------------------------------
        // The exact UTC date and time when the event object was created.
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        // ---------------------------------------------------------------------
        // Correlation Identifier (Trace ID)
        // ---------------------------------------------------------------------
        // This optional string allows tracking of a single business transaction
        // across multiple microservices.

        // Example:
        //   - When a customer places an order, the order service assigns a CorrelationId.
        //   - All subsequent events (StockReservationRequested, StockReservedCompleted,
        //     OrderConfirmed, etc.) reuse the same CorrelationId.

        // This makes it possible to trace the entire flow of a request end-to-end
        // across different services in logs, monitoring tools (like Kibana or Application Insights),
        // or distributed tracing systems (like OpenTelemetry or Jaeger).
        public string? CorrelationId { get; set; }
    }
}
