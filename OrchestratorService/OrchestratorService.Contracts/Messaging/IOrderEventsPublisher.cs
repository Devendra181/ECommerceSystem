using Messaging.Common.Events;
namespace OrchestratorService.Contracts.Messaging
{
    // Purpose:
    //   Defines the abstraction for publishing Saga-related events
    //   from the OrchestratorService to RabbitMQ.
    //
    // Design Notes:
    //   - Concrete implementation resides in OrchestratorService.Infrastructure.Messaging.
    public interface IOrderEventsPublisher
    {
        // Method: PublishStockReservationRequestedAsync
        // Description:
        //   Publishes a "StockReservationRequestedEvent" message
        //   to ask ProductService to check and reserve inventory
        //   for the given order.
        //
        // When Called:
        //   - Immediately after the Orchestrator consumes an OrderPlacedEvent
        //     from the OrderService.
        //
        // Role in Saga Flow:
        //   This marks the transition from the "order created" step
        //   to the "inventory check" step in the Saga.
        //
        // Downstream Consumer:
        //   - ProductService → consumes this event and attempts to reserve stock.
        //
        // Parameters:
        //   evt → Contains details such as OrderId, UserId, and Items to reserve.
        //
        // Outcome:
        //   Begins the stock reservation phase of the distributed transaction.
        Task PublishStockReservationRequestedAsync(StockReservationRequestedEvent evt);

        // Method: PublishOrderConfirmedAsync
        // Description:
        //   Publishes an "OrderConfirmedEvent" indicating that
        //   the order has been successfully processed — stock
        //   reserved, and no compensation required.
        //
        // When Called:
        //   - After the Orchestrator receives a StockReservedCompletedEvent
        //     from ProductService.
        //
        // Role in Saga Flow:
        //   This marks the "success path" of the Saga.
        //
        // Downstream Consumers:
        //   - NotificationService → sends confirmation email/SMS to the customer.
        //
        // Parameters:
        //   evt → Includes full order details such as OrderId, Customer info,
        //          and reserved item list.
        //
        // Outcome:
        //   Completes the Saga successfully — the order is confirmed.
        Task PublishOrderConfirmedAsync(OrderConfirmedEvent evt);

        // Method: PublishOrderCancelledAsync
        // Description:
        //   Publishes an "OrderCancelledEvent" indicating that
        //   the order has failed due to stock unavailability or
        //   another orchestration rule (e.g., payment or validation failure).
        //
        // When Called:
        //   - After the Orchestrator receives a StockReservationFailedEvent
        //     from ProductService.
        //
        // Role in Saga Flow:
        //   This marks the "compensation path" of the Saga.
        //
        // Downstream Consumers:
        //   - OrderService → compensates by marking the order as Cancelled.
        //   - NotificationService → informs the customer about the failure reason.
        //
        // Parameters:
        //   evt → Includes OrderId, Customer info, list of failed items,
        //          and the failure Reason (e.g., "Insufficient Stock").
        //
        // Outcome:
        //   Saga completes with compensation — order cancelled.
        Task PublishOrderCancelledAsync(OrderCancelledEvent evt);
    }
}
