using Messaging.Common.Events;
using Microsoft.Extensions.Logging;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Messaging
{
    // Purpose:
    //   Implements the IOrderCancelledHandler interface to execute
    //   the compensation logic in the Saga Orchestration flow.
    //
    //   When the OrchestratorService determines that an order cannot
    //   be completed (e.g., stock reservation or payment failure),
    //   it publishes an "OrderCancelledEvent".
    //
    //   The OrderCancelledConsumer (in Infrastructure layer) receives
    //   this message and delegates the work to this handler.
    //
    //   This handler performs the actual business operation by marking
    //   the order as "Cancelled" in the database and logging the outcome.
    //
    // Design Notes:
    //   - The Application layer is responsible for business logic.
    //   - The Infrastructure layer only handles message delivery.
    public sealed class OrderCancelledHandler : IOrderCancelledHandler
    {
        // Field: _orderRepository
        //   Provides data access methods for updating order records
        //   in the database. Used here to change the order status to "Cancelled".
        private readonly IOrderRepository _orderRepository;

        // Field: _logger
        //   Used to log key actions and results during compensation.
        //   Helps trace when and why an order was cancelled.
        private readonly ILogger<OrderCancelledHandler> _logger;

        // Constructor:
        // Parameters:
        //   - orderRepository → Repository abstraction to perform database operations.
        //   - logger          → Logger used for tracking execution and errors.
        public OrderCancelledHandler(
            IOrderRepository orderRepository,
            ILogger<OrderCancelledHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Method: HandleAsync
        // Description:
        //   Executes the compensation logic when an OrderCancelledEvent
        //   is received from the OrchestratorService.
        //
        //   This method updates the order status in the database to "Cancelled"
        //   and logs important details for traceability and debugging.
        //
        // Flow:
        //   1. Receive cancellation event from Orchestrator (via consumer).
        //   2. Log the start of the compensation process with OrderId and reason.
        //   3. Update the order’s status in the database (Cancelled).
        //   4. Log the successful completion or throw exception if failed.
        //
        // Parameters:
        //   - message       → The event data received (OrderId, Reason, etc.).
        //   - correlationId → Unique ID to trace this cancellation across
        //                     multiple services in the Saga transaction.
        //
        // Return Type:
        //   Task → Asynchronous operation for non-blocking DB update.
        //
        // Notes:
        //   - If the DB update fails, the exception ensures visibility in logs.
        //   - RabbitMQ will handle retries or move the message to a dead-letter queue.
        public async Task HandleAsync(OrderCancelledEvent message)
        {
            // Log the start of the compensation process.
            // Includes correlationId for end-to-end Saga tracing.
            _logger.LogInformation($"Starting compensation for OrderId: {message.OrderId}, Reason: {message.Reason}");

            // Attempt to change the order’s status in the database to “Cancelled”.
            // This is the key compensation step that ensures consistency
            // after a distributed transaction failure.
            var ok = await _orderRepository.ChangeOrderStatusAsync(
                message.OrderId,
                OrderStatusEnum.Cancelled,     // Set new status
                changedBy: "Orchestrator",     // For audit tracking: who initiated the change
                remarks: message.Reason);      // Reason provided by Orchestrator (e.g., "Stock unavailable")

            // Check if the update was successful.
            // If not, throw an exception so the failure is logged and retried.
            if (!ok)
            {
                throw new InvalidOperationException(
                    $"Failed to update order {message.OrderId} status to Cancelled.");
            }

            // Log successful completion of compensation.
            // Confirms that the order has been properly marked as Cancelled.
            _logger.LogInformation(
                "Order compensation successful for OrderId: {OrderId}", message.OrderId);
        }
    }
}

