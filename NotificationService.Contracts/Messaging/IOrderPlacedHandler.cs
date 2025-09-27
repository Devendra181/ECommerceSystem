using Messaging.Common.Events;
namespace NotificationService.Contracts.Messaging
{
    public interface IOrderPlacedHandler
    {
        Task HandleAsync(OrderPlacedEvent evt);
    }
}
