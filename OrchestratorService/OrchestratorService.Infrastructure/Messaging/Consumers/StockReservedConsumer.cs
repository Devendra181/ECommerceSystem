using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchestratorService.Contracts.Messaging;
using RabbitMQ.Client;
namespace OrchestratorService.Infrastructure.Messaging.Consumers
{
    // Purpose:
    //   This consumer listens to RabbitMQ messages published by ProductService
    //   after stock has been successfully reserved.
    //
    //   In other words, it represents the "success path" in the Saga flow.
    //   Once ProductService publishes a StockReservedCompletedEvent,
    //   this consumer triggers the Orchestrator to finalize the order
    //   by publishing an OrderConfirmedEvent.
    //
    // Message Flow:
    //   [ProductService] → publishes StockReservedCompletedEvent
    //        ↓
    //   [OrchestratorService] → consumes it via StockReservedConsumer
    //        ↓
    //   Calls OrchestrationService.OnStockReservedAsync()
    //        ↓
    //   Publishes OrderConfirmedEvent → (NotificationService react)
    public sealed class StockReservedConsumer : BaseConsumer<StockReservedCompletedEvent>
    {
        // IServiceScopeFactory is required because this consumer runs as a hosted background service.
        // We cannot inject scoped dependencies (like DbContext or OrchestrationService) directly.
        // Instead, we create a new dependency injection scope for each incoming message.
        private readonly IServiceScopeFactory _scopeFactory;

        // --------------------------------------------------------
        // Constructor
        // Parameters:
        //   - channel: RabbitMQ channel (IModel) used for consuming messages.
        //   - queueName: Name of the queue bound to the routing key "stock.reserved".
        //   - scopeFactory: Creates DI scopes for safely resolving scoped services per message.
        //   - logger: Used for structured logging (provided to BaseConsumer).
        // --------------------------------------------------------
        public StockReservedConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<StockReservedConsumer> logger)
            : base(channel, queueName, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // --------------------------------------------------------
        // Method: HandleMessage
        // Description:
        //   Invoked automatically by BaseConsumer whenever a
        //   StockReservedCompletedEvent is received and deserialized.
        //
        // Responsibilities:
        //   1️. Create a new scoped lifetime for this message (fresh DI scope).
        //   2️. Resolve IOrchestrationService from the scoped provider.
        //   3️. Delegate processing to OnStockReservedAsync(), which:
        //         → Retrieves cached order info (OrderPlacedEvent).
        //         → Publishes OrderConfirmedEvent.
        //         → Removes order from cache (Saga complete).
        //
        // Outcome:
        //   A successful stock reservation triggers an OrderConfirmedEvent,
        //   allowing downstream services (OrderService, NotificationService)
        //   to finalize the workflow.
        //
        // Error Handling:
        //   - If no exception: message is ACKed (successfully processed).
        //   - If exception: message is NACKed (goes to DLQ or requeue, based on config).
        // --------------------------------------------------------
        protected override async Task HandleMessage(StockReservedCompletedEvent message)
        {
            // Create a DI scope for this specific message.
            // Ensures any scoped services (e.g., DbContext or OrchestrationService)
            // are instantiated fresh and disposed after use.
            using var scope = _scopeFactory.CreateScope();

            // Resolve the OrchestrationService (application layer).
            // This service contains the main Saga logic for handling success cases.
            var orchestrator = scope.ServiceProvider.GetRequiredService<IOrchestrationService>();

            // Delegate processing to the orchestrator logic.
            // It will:
            //   - Retrieve the cached OrderPlacedEvent (from memory/Redis).
            //   - Publish an OrderConfirmedEvent.
            //   - Clean up the Saga state for this order.
            await orchestrator.OnStockReservedAsync(message);

            // If successful:
            //   - BaseConsumer automatically sends an ACK to RabbitMQ.
            //   - This marks the message as processed and removes it from the queue.
        }
    }
}
