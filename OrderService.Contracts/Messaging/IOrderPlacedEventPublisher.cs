using Messaging.Common.Events;
namespace OrderService.Contracts.Messaging
{
    // Layer: Contracts (shared between Application and Infrastructure)

    // Purpose:
    //   Defines a contract for publishing the "OrderPlacedEvent" message.
    //   This event is raised by the OrderService when a new order is successfully created.
    //   It notifies the OrchestratorService to start the Saga workflow that manages
    //   coordination among other microservices (like ProductService, PaymentService,
    //   and NotificationService).

    // Why Interface?
    //   Using an interface decouples the Application layer from the
    //   actual RabbitMQ implementation. The Application layer only depends
    //   on this abstraction, while the Infrastructure layer provides
    //   the concrete publishing logic.
    public interface IOrderPlacedEventPublisher
    {
        // Method: PublishOrderPlacedAsync

        // Description:
        //   Publishes the "OrderPlacedEvent" to the message broker (RabbitMQ).
        //   This marks the starting point of the Saga Orchestration flow.
        //   Once published, the OrchestratorService receives this event and
        //   coordinates the following actions:
        //     - Requests ProductService to reserve stock.
        //   Based on the outcome, the Orchestrator may publish
        //   either an OrderConfirmedEvent or an OrderCancelledEvent.

        // Parameters:
        //   evt            → The event payload containing order details such as
        //                     OrderId, UserId, TotalAmount, and list of ordered items.

        // Return Type:
        //   Task → Represents an asynchronous operation, ensuring that
        //           publishing can be awaited without blocking threads.

        // Notes:
        //   - The Infrastructure layer (OrderPlacedEventPublisher class)
        //     implements this interface and handles RabbitMQ publishing.
        //   - The Application layer calls this method after the order
        //     is successfully saved in the database.
        //   - The OrchestratorService, not other microservices directly,
        //     receives this event first and controls the next steps
        //     in the Saga coordination process.
        Task PublishOrderPlacedAsync(OrderPlacedEvent evt);
    }
}
