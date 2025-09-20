using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using OrderService.Contracts.Messaging;

namespace OrderService.Infrastructure.Messaging
{
    public sealed class RabbitMqOrderEventPublisher : IOrderEventPublisher
    {
        private readonly IModel _channel;        // RabbitMQ channel object, used to publish messages.
        private readonly RabbitMqOptions _opt;   // Holds RabbitMQ configuration settings.

        // Constructor: dependencies (channel + options) are injected via DI.
        public RabbitMqOrderEventPublisher(IModel channel, IOptions<RabbitMqOptions> opt)
        {
            _channel = channel;                  // Assign the RabbitMQ channel (already created and registered).
            _opt = opt.Value;                    // Extract RabbitMQ configuration values (host, exchange, queues, etc.).

            // Ensure the exchange, queues, and bindings exist before publishing.
            // This avoids publishing to a non-existent exchange/queue.
            RabbitTopology.EnsureAll(_channel, _opt);
        }

        // Publishes an OrderPlacedEvent message to RabbitMQ.
        // correlationId helps trace the message across multiple microservices.
        public Task PublishOrderPlacedAsync(OrderPlacedEvent evt, string? correlationId = null)
        {
            // If no correlationId was provided, use the one from the event.
            evt.CorrelationId = correlationId ?? evt.CorrelationId;

            // Serialize the event object into JSON UTF-8 bytes (efficient for transport).
            var body = JsonSerializer.SerializeToUtf8Bytes(evt);

            // Create RabbitMQ message properties (metadata for the message).
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;             // Ensures the message is persisted to disk (survives broker restart).
            props.CorrelationId = evt.CorrelationId;  // Set correlationId for traceability.

            // Publish the message to RabbitMQ:
            // - Exchange: taken from configuration (e.g., "ecommerce.topic").
            // - RoutingKey: "order.placed" ensures it is delivered to bound queues (product & notification).
            // - Mandatory = true: if no queue is bound to the routing key, the message is returned or dead-lettered.
            // - Properties: message metadata (persistence, correlationId).
            // - Body: the serialized OrderPlacedEvent payload.
            _channel.BasicPublish(_opt.ExchangeName, "order.placed", true, props, body);

            // Return a completed task (since this is a fire-and-forget publish).
            return Task.CompletedTask;
        }
    }
}
