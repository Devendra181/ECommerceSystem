using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Messaging.Common.Publishing
{
    public class Publisher
    {
        // The RabbitMQ channel (IModel) used to send messages.
        private readonly IModel _channel;

        // Constructor: requires a RabbitMQ channel (injected from DI).
        public Publisher(IModel channel)
        {
            _channel = channel;
        }

        // Publishes a message to a RabbitMQ exchange with a given routing key.
        // T: Type of the message to publish.
        // exchange: Exchange name (e.g., ecommerce_exchange).
        // routingKey: Routing key used for queue binding (e.g., order.placed).
        // message: The message object (will be serialized to JSON).
        // correlationId: Optional unique ID for tracing.
        public void Publish<T>(string exchange, string routingKey, T message, string? correlationId = null)
        {
            // Serialize the message object into JSON, then encode into UTF-8 byte array.
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Create message properties.
            var props = _channel.CreateBasicProperties();
            props.Persistent = true; // Makes message persistent (saved to disk).
            props.CorrelationId = correlationId ?? Guid.NewGuid().ToString();

            // Publish the message to RabbitMQ.
            _channel.BasicPublish(
                exchange: exchange,          // Exchange name
                routingKey: routingKey,      // Routing key (routes message to queues)
                basicProperties: props,      // Properties (persistence + correlationId)
                body: body                   // The actual message payload
            );
        }
    }
}
