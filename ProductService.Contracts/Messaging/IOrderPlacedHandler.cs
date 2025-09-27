using Messaging.Common.Events;
namespace ProductService.Contracts.Messaging
{
    public interface IOrderPlacedHandler
    {
        Task HandleAsync(OrderPlacedEvent evt);
    }
}
