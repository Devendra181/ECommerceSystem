using Messaging.Common.Events;
namespace OrderService.Contracts.Messaging
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderPlacedAsync(OrderPlacedEvent evt, string? correlationId = null);
    }
}
