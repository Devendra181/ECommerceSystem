using Messaging.Common.Events;

namespace OrderService.Contracts.Messaging
{
    // Layer: Contracts (shared between Application and Infrastructure)

    // Purpose:
    //   Defines a contract for handling the "OrderCancelledEvent" message.
    //   When the Orchestrator publishes an order cancellation event (for example,
    //   due to stock unavailability), the OrderService must perform a
    //   compensation action such as updating the order status in its database.

    // Why Interface?
    //   Using an interface ensures that the Application layer (business logic)
    //   implements the compensation behavior, while the Infrastructure layer
    //   simply triggers it — promoting clean separation of concerns.

    public interface IOrderCancelledHandler
    {
        //HandleAsync Method
        //   Executes the compensation logic when an order cancellation event
        //   is received from RabbitMQ (published by the Orchestrator Service).
        //   The implementation of this method will update the order’s status
        //   (e.g., from "Pending" or "Confirmed" to "Cancelled") in the database.

        // Parameters:
        //   message      → The event payload (OrderCancelledEvent) that contains
        //                  details like OrderId, Reason for cancellation, etc.

        // Return Type:
        //   Task → Indicates that the operation is asynchronous.

        // Notes:
        //   - This method will be implemented in the Application layer.
        //   - The Infrastructure layer (consumer) will call this method after
        //     deserializing the message from RabbitMQ.
        Task HandleAsync(OrderCancelledEvent message);
    }
}
