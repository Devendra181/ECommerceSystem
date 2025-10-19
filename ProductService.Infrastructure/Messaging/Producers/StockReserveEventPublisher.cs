using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Microsoft.Extensions.Options;
using ProductService.Contracts.Messaging;

namespace ProductService.Infrastructure.Messaging.Producers
{
    // Responsible for publishing stock reservation outcome events from ProductService to RabbitMQ.

    // When ProductService completes stock reservation (success or failure),
    // this class sends the corresponding event message to the correct exchange
    // and routing key defined in RabbitMqOptions.

    // Published Events:
    //     1. StockReservedCompletedEvent  → when stock reservation is successful.
    //     2. StockReservationFailedEvent  → when stock reservation fails.
    // 
    // These messages are consumed by OrchestratorService to continue the Saga flow.

    public sealed class StockReserveEventPublisher : IStockReserveEventPublisher
    {
        // Shared RabbitMQ publisher abstraction (from Messaging.Common)
        // Handles serialization, delivery mode, and correlation ID internally.
        private readonly IPublisher _publisher;

        // RabbitMQ configuration settings (exchange names, routing keys, etc.)
        private readonly RabbitMqOptions _opt;

        // Constructor that injects the generic publisher and configuration options.
        public StockReserveEventPublisher(IPublisher publisher, IOptions<RabbitMqOptions> options)
        {
            _publisher = publisher;
            _opt = options.Value; // Extracts configuration values (e.g., exchange name, routing keys)
        }

        // Publishes a StockReservedCompletedEvent message when stock reservation succeeds.
        // The message is sent to the main topic exchange using the routing key "stock.reserved".
        // OrchestratorService’s StockReservedConsumer listens for this event to confirm the order.
        public Task PublishStockReservedCompletedAsync(StockReservedCompletedEvent evt)
        {
            // Sends message to RabbitMQ topic exchange with routing key (stock.reserved)
            return _publisher.PublishAsync(
                _opt.ExchangeName,  // e.g., "ecommerce.topic"
                _opt.RkStockReserved, // e.g., "stock.reserved"
                evt                  // Event payload (JSON serialized)
            );
        }

        // Publishes a StockReservationFailedEvent message when stock reservation fails.
        // This event notifies the OrchestratorService that ProductService could not
        // reserve stock due to insufficient quantity or missing products.
        // The Orchestrator then publishes an OrderCancelledEvent to compensate.
        public Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt)
        {
            // Sends message to RabbitMQ topic exchange with routing key (stock.reservation_failed)
            return _publisher.PublishAsync(
                _opt.ExchangeName, // e.g., "ecommerce.topic"
                _opt.RkStockFailed, // e.g., "stock.reservation_failed"
                evt                 // Event payload (contains failure reason and failed items)
            );
        }
    }
}
