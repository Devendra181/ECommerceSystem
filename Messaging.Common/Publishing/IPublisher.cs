namespace Messaging.Common.Publishing
{
    public interface IPublisher
    {
        Task PublishAsync(string exchange, string routingKey, object message, string? correlationId = null);
    }
}
