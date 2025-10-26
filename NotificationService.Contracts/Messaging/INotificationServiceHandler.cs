using Messaging.Common.Events;

namespace NotificationService.Contracts.Messaging
{
    //    This interface defines the high-level contract for the NotificationService
    //    within the Saga Orchestration pattern.
    //    It acts as a bridge between the infrastructure layer (consumers)
    //    and the application layer (where notification logic is executed).
    public interface INotificationServiceHandler
    {
        // HandleOrderConfirmedAsync:
        // This method is called when an OrderConfirmedEvent is received from RabbitMQ.
        // It contains details like OrderId, CustomerName, Email, etc.
        // The implementation will send a "success" notification (email/SMS) to the customer
        // confirming that their order was successfully processed.
        Task HandleOrderConfirmedAsync(OrderConfirmedEvent evt);

        // HandleOrderCancelledAsync:
        // This method is called when an OrderCancelledEvent is received from RabbitMQ.
        // It carries cancellation details such as the reason for failure (e.g., insufficient stock).
        // The implementation will send a "failure/cancellation" notification to the customer
        // informing them that the order could not be completed.
        Task HandleOrderCancelledAsync(OrderCancelledEvent evt);
    }
}
