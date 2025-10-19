namespace Messaging.Common.Publishing
{
    // Defines a generic contract for publishing messages to a message broker (RabbitMQ in this case).
    // Using an interface ensures loose coupling — meaning your services depend on this abstraction,
    // not directly on RabbitMQ. Later, you could easily replace RabbitMQ with another message broker
    // (like Kafka, Azure Service Bus, etc.) without changing the rest of your code.
    public interface IPublisher
    {
        // Publishes a message asynchronously to the given exchange with the specified routing key.
        //      exchange: The exchange name to which the message will be sent.
        //      routingKey: The routing key that determines which queue(s) receive the message..
        //      message: The actual message payload to publish (usually an event object =serialized to JSON).
        Task PublishAsync(string exchange, string routingKey, object message);
    }
}
