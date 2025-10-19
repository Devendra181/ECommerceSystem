using Messaging.Common.Events;
namespace OrchestratorService.Contracts.Messaging
{
    // Purpose:
    //   Defines the core decision-making contract for the OrchestratorService
    //   in a Saga-based distributed transaction.
    //
    //   The Orchestrator coordinates between multiple microservices
    //   (OrderService, ProductService, PaymentService, NotificationService, etc.)
    //   to ensure that all services reach a consistent outcome for a given order.
    //
    //   Each method below represents a "decision point" or event reaction
    //   in the overall Saga workflow.
    //
    // Design Notes:
    //   - OrchestratorService.Application.Services.OrchestrationService implements this interface.
    //   - Event consumers (OrderPlacedConsumer, StockReservedConsumer, etc.)
    //     call these methods whenever their respective event is received.
    public interface IOrchestrationService
    {
        // Method: OnOrderPlacedAsync
        // Description:
        //   Handles the start of the Saga workflow when a new order is created.
        //   Triggered when the OrderService publishes an OrderPlacedEvent.
        //
        // Behavior:
        //   1. Validate and cache the received OrderPlacedEvent.
        //   2. Publish a StockReservationRequestedEvent to the ProductService.
        //   3. Wait for a response (StockReserved or StockReservationFailed).
        //
        // Parameters:
        //   evt → The OrderPlacedEvent containing order and customer details.
        //
        // Outcome:
        //   Starts the distributed transaction by requesting inventory reservation.
        Task OnOrderPlacedAsync(OrderPlacedEvent evt);

        // Method: OnStockReservedAsync
        // Description:
        //   Handles the happy (success) path of the Saga.
        //   Triggered when ProductService successfully reserves stock
        //   and publishes a StockReservedCompletedEvent.
        //
        // Behavior:
        //   1. Retrieve cached order details (from OnOrderPlacedAsync).
        //   2. Publish an OrderConfirmedEvent so:
        //        - OrderService updates order status to Confirmed.
        //        - NotificationService sends confirmation to the customer.
        //   3. Remove cached order data (since the Saga completed successfully).
        //
        // Parameters:
        //   evt → The StockReservedCompletedEvent containing order ID and reserved items.
        //
        // Outcome:
        //   Saga completes successfully (order confirmed, notifications sent).
        Task OnStockReservedAsync(StockReservedCompletedEvent evt);

        // Method: OnStockReservationFailedAsync
        // Description:
        //   Handles the failure (compensation) path of the Saga.
        //   Triggered when ProductService cannot reserve stock and
        //   publishes a StockReservationFailedEvent.
        //
        // Behavior:
        //   1. Retrieve cached order details (from OnOrderPlacedAsync).
        //   2. Publish an OrderCancelledEvent so:
        //        - OrderService compensates by marking order as Cancelled.
        //        - NotificationService informs the customer of the failure.
        //   3. Remove cached order data (since the Saga is finalized).
        //
        // Parameters:
        //   evt → The StockReservationFailedEvent containing order ID,
        //          reason for failure, and failed item details.
        //
        // Outcome:
        //   Saga ends with compensation (order cancelled gracefully).
        Task OnStockReservationFailedAsync(StockReservationFailedEvent evt);
    }
}
