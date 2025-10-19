using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Microsoft.Extensions.Options;
using OrderService.Contracts.Messaging;
namespace OrderService.Infrastructure.Messaging.Producers
{
    // Purpose:
    //   Implements the IOrderPlacedEventPublisher interface.
    //   Responsible for publishing the "OrderPlacedEvent" message
    //   to RabbitMQ after a new order is successfully created.

    //   In the Orchestration-based Saga pattern, this event is the starting point.
    //   It is consumed by the OrchestratorService,
    //   which coordinates the overall workflow across microservices
    //   (ProductService, PaymentService, NotificationService, etc.).
    public sealed class OrderPlacedEventPublisher : IOrderPlacedEventPublisher
    {
        // Field: _publisher
        // Description:
        //   Shared publishing abstraction from Messaging.Common.
        //   Handles all low-level RabbitMQ publishing operations such as:
        //     - Message serialization (usually JSON)
        //     - Adding correlation IDs for distributed tracing
        //     - Ensuring message persistence and reliability
        //   Keeps the OrderService free from RabbitMQ-specific logic.
        private readonly IPublisher _publisher;

        // Field: _options
        // Description:
        //   Strongly-typed configuration settings for RabbitMQ.
        //   Includes:
        //     - Exchange name (e.g., "ecommerce.topic")
        //     - Routing keys (e.g., "order.placed")
        //     - Queue names and connection details
        //   Centralized and configurable message routing without hardcoding values.
        private readonly RabbitMqOptions _options;

        // Constructor:
        //   Injects dependencies via .NET's built-in dependency injection (DI).

        // Parameters:
        //   - publisher → IPublisher abstraction for publishing messages.
        //   - options   → IOptions<RabbitMqOptions> to access RabbitMQ configuration.
        public OrderPlacedEventPublisher(IPublisher publisher, IOptions<RabbitMqOptions> options)
        {
            _publisher = publisher;
            _options = options.Value;
        }

        // Method: PublishOrderPlacedAsync
        // Description:
        //   Publishes the "OrderPlacedEvent" to RabbitMQ.
        //   This event informs the OrchestratorService that a new order
        //   has been placed and that it should begin the Saga workflow.

        //   The OrchestratorService then:
        //     - Sends a "StockReservationRequestedEvent" to ProductService.

        // Parameters:
        //   evt            → The actual event data containing order details like
        //                    OrderId, UserId, TotalAmount, and Item details.

        // Return:
        //   Task → Represents the asynchronous publish operation.
        public Task PublishOrderPlacedAsync(OrderPlacedEvent evt)
        {
            // Log (for development/debugging purposes only).
            // Helps confirm that the event has been triggered successfully.
            Console.WriteLine($"[Publish] OrderPlacedEvent sent for OrderId={evt.OrderId}");

            // Publish the event using the shared IPublisher abstraction.
            // - exchange:   The central topic exchange shared across all services, e.g., "ecommerce.topic"

            // - routingKey: Determines which queue(s) will receive the event.
            //               For this event, the key is typically "order.placed".

            // - message:    The actual event payload to be serialized and sent.

            // This message will be consumed first by the OrchestratorService,
            // which coordinates all subsequent microservice operations.
            return _publisher.PublishAsync(
                exchange: _options.ExchangeName,     // e.g., "ecommerce.topic"
                routingKey: _options.RkOrderPlaced,  // e.g., "order.placed"
                message: evt                        // event payload (OrderPlacedEvent)
            );
        }
    }
}
