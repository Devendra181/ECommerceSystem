using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchestratorService.Contracts.Messaging;
using RabbitMQ.Client;
namespace OrchestratorService.Infrastructure.Messaging.Consumers
{
    // Purpose:
    //   This consumer listens for the "StockReservationFailedEvent" message
    //   published by ProductService when stock reservation cannot be completed.
    //
    //   Example scenarios:
    //   - Product is out of stock
    //   - Insufficient quantity available
    //   - Invalid or missing product entries
    //
    //   Once this consumer receives the event, it triggers the
    //   OrchestrationService to perform *compensating actions*:
    //      → Publish an OrderCancelledEvent
    //      → Notify OrderService to mark order as Cancelled
    //      → Notify NotificationService to inform the user
    //
    //   This represents the failure path of the Saga workflow.
    public sealed class StockReservationFailedConsumer : BaseConsumer<StockReservationFailedEvent>
    {
        // IServiceScopeFactory is required because this consumer
        // runs as a background hosted service and cannot directly
        // inject scoped services (like DbContext or OrchestrationService).
        // Each message processed gets its own DI scope for safety.
        private readonly IServiceScopeFactory _scopeFactory;

        // --------------------------------------------------------
        // Constructor
        // Parameters:
        //   - channel: RabbitMQ channel (IModel) used to receive messages.
        //   - queueName: Queue name bound to routing key "stock.failed"
        //                (configured via RabbitMqOptions).
        //   - scopeFactory: Enables creation of a new DI scope per message.
        //   - logger: Used for structured logging and tracing.
        // --------------------------------------------------------
        public StockReservationFailedConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<StockReservationFailedConsumer> logger)
            : base(channel, queueName, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // --------------------------------------------------------
        // Method: HandleMessage
        //
        // Description:
        //   Called automatically by BaseConsumer when a new
        //   StockReservationFailedEvent is received and deserialized.
        //
        //   This method handles the compensation part of the Saga flow.
        //
        // Workflow Steps:
        //   1️. Create a new dependency injection scope.
        //   2️. Resolve IOrchestrationService from DI container.
        //   3️. Call OnStockReservationFailedAsync() to perform compensation.
        //
        // Responsibilities of OrchestrationService:
        //   → Retrieve cached OrderPlacedEvent (from memory or Redis).
        //   → Publish OrderCancelledEvent (with detailed failure reason).
        //   → Remove the order from cache (saga cleanup).
        //
        // Outcome:
        //   OrderService and NotificationService are informed of cancellation.
        //
        // Error Handling:
        //   - Success → message is ACKed (removed from queue).
        //   - Exception → BaseConsumer automatically NACKs
        //     the message (goes to DLX or requeue, depending on setup).
        // --------------------------------------------------------
        protected override async Task HandleMessage(StockReservationFailedEvent message)
        {
            // Create a scoped lifetime for this specific message.
            // This ensures that any scoped services (like DbContext or
            // OrchestrationService) are fresh instances, and disposed
            // properly after the message is processed.
            using var scope = _scopeFactory.CreateScope();

            // Resolve OrchestrationService from DI container.
            // This application-layer service contains the business logic
            // for handling failed stock reservations in the Saga flow.
            var orchestrator = scope.ServiceProvider.GetRequiredService<IOrchestrationService>();

            // Delegate failure handling to OrchestrationService.
            // This triggers:
            //   - OrderCancelledEvent publication
            //   - Order status compensation in OrderService
            //   - Customer notification via NotificationService
            await orchestrator.OnStockReservationFailedAsync(message);

            // BaseConsumer will automatically ACK this message if
            //     processing completes successfully.
            // If any exception occurs, BaseConsumer will NACK the message,
            //     allowing RabbitMQ to handle retries or DLQ routing.
        }
    }
}
