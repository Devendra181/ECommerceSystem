using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Messaging.Common.Publishing
{
    // The default implementation of IPublisher that sends messages to RabbitMQ.
    // Responsibilities:
    //  - Serialize message objects to JSON.
    //  - Set message properties (like persistence, content type, correlation ID).
    //  - Publish the message to the specified exchange and routing key.
    public sealed class Publisher : IPublisher
    {
        // ---------------------------------------------------------------------
        // Private Fields
        // ---------------------------------------------------------------------
        // Represents a RabbitMQ channel (IModel) used for communication with the broker.
        // It’s used for performing operations (declare, publish, consume, etc.).
        private readonly IModel _channel;

        // ---------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------
        // Initializes the publisher with an active RabbitMQ channel.
        // The channel is injected via dependency injection (registered in DI container).
        // Parameter:
        //      channel: An open RabbitMQ channel used for publishing messages.
        public Publisher(IModel channel)
        {
            _channel = channel;
        }

        // ---------------------------------------------------------------------
        // PublishAsync Method
        // ---------------------------------------------------------------------
        // Publishes a message to RabbitMQ using the provided exchange and routing key.
        //      exchange: The target exchange to publish the message to.
        //      routingKey: The routing key that determines which queue(s) will receive the message.
        //      message: The actual object (event DTO) to send.
        //      correlationId: Optional unique ID for tracing message flow across services.
        public Task PublishAsync(string exchange, string routingKey, object message)
        {
            // ----------------------------------------------------------
            // Step 1: Serialize the message payload to JSON format
            // ----------------------------------------------------------
            // Converts the .NET object (e.g., OrderPlacedEvent) into a JSON string
            // so it can be sent over RabbitMQ (which only transmits byte arrays).
            var json = JsonSerializer.Serialize(message);

            // Convert the JSON string into a UTF-8 byte array (RabbitMQ requires binary payloads).
            var body = Encoding.UTF8.GetBytes(json);

            // ----------------------------------------------------------
            // Step 2: Create and configure basic message properties
            // ----------------------------------------------------------
            // Properties allow you to set metadata like content type, persistence, correlation ID, etc.
            var props = _channel.CreateBasicProperties();

            // Specify that the message content is in JSON format.
            props.ContentType = "application/json";

            // Make the message persistent (DeliveryMode = 2).
            // Persistent messages are stored on disk by RabbitMQ,
            // so they survive broker restarts (as long as the queue is durable too).
            props.DeliveryMode = 2;

            // ----------------------------------------------------------
            // Step 3: Publish the message to RabbitMQ
            // ----------------------------------------------------------
            // Sends the serialized message to the target exchange using the routing key.
            // The routing key determines which queue(s) the message will reach.
            // Parameters:
            //  - exchange: Name of the exchange (topic, direct, etc.)
            //  - routingKey: Used by RabbitMQ to route to the correct queue(s)
            //  - basicProperties: Metadata attached to the message (props)
            //  - body: Actual message data in bytes
            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: props,
                body: body
            );

            //What happens when we call _ch.BasicPublish
            //  This sends the message to RabbitMQ immediately using the open channel.
            //  The method BasicPublish() from the RabbitMQ .NET client does not return a Task
            //  or wait for confirmation that the broker received the message,
            //  it just pushes it to the TCP connection and returns instantly.

            //  So from.NET’s point of view, the publishing operation is completed the moment that call returns,
            //  even though the broker might still be processing it internally.

            //  In short: There is no asynchronous work(no await, no background operation) happening after that line.

            // ----------------------------------------------------------
            // Step 4: Complete the Task
            // ----------------------------------------------------------
            // The method doesn’t await any I/O, so we return a completed task.
            // This method is already done — return a completed Task to satisfy the async method signature.
            return Task.CompletedTask;
        }
    }
}

