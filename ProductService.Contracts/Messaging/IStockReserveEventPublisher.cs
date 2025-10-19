
using Messaging.Common.Events;

namespace ProductService.Contracts.Messaging
{
    // Defines the contract for publishing stock reservation–related events
    // from the ProductService to the message broker (RabbitMQ).
    // 
    // This interface abstracts away the message publishing logic so that
    // the ProductService can focus on business decisions (reserve/fail)
    // instead of message transport details.
    public interface IStockReserveEventPublisher
    {
        // Publishes a StockReservedCompletedEvent to RabbitMQ
        // when the ProductService successfully reserves stock for all requested items.
        // 
        // This event is consumed by the OrchestratorService, which then
        // continues the Saga flow by confirming the order (publishes OrderConfirmedEvent).
        Task PublishStockReservedCompletedAsync(StockReservedCompletedEvent evt);

        // Publishes a StockReservationFailedEvent to RabbitMQ
        // when the ProductService cannot reserve stock (e.g., insufficient quantity).
        // 
        // This event signals the OrchestratorService to trigger compensation logic
        // publishes OrderCancelledEvent to inform OrderService and NotificationService.
        Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt);
    }
}
