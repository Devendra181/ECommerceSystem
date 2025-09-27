using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Messaging.Common.Publishing
{
    // Default RabbitMQ publisher:
    // - JSON serialize payload
    // - Persistent delivery mode
    // - Sets CorrelationId for tracing
    public sealed class Publisher : IPublisher
    {
        // The RabbitMQ channel (IModel) used to send messages.
        private readonly IModel _ch;

        // Constructor: requires a RabbitMQ channel (injected from DI).
        public Publisher(IModel channel) => _ch = channel;

        // Publishes a message to a RabbitMQ exchange with a given routing key.
        // T: Type of the message to publish.
        // exchange: Exchange name (e.g., ecommerce_exchange).
        // routingKey: Routing key used for queue binding (e.g., order.placed).
        // message: The message object (will be serialized to JSON).
        // correlationId: Optional unique ID for tracing.
        public Task PublishAsync(string exchange, string routingKey, object message, string? correlationId = null)
        {
            // Serialize the message object into JSON, then encode into UTF-8 byte array.
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _ch.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; // persistent
            if (!string.IsNullOrWhiteSpace(correlationId))
                props.CorrelationId = correlationId;

            _ch.BasicPublish(
                exchange: exchange, 
                routingKey: routingKey, 
                basicProperties: props, 
                body: body
                );
            return Task.CompletedTask;
        }
    }
}
