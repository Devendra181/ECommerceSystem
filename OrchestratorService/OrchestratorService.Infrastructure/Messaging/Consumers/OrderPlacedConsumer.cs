using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchestratorService.Contracts.Messaging;
using RabbitMQ.Client;
namespace OrchestratorService.Infrastructure.Messaging.Consumers
{
    // Purpose:
    //   This consumer listens to the RabbitMQ queue bound to the
    //   "order.placed" routing key.
    //
    //   When OrderService publishes an OrderPlacedEvent (after saving
    //   a new order in its DB), this consumer receives it and delegates
    //   processing to the OrchestrationService.
    //
    //   The OrchestrationService then starts the Saga workflow by
    //   publishing a StockReservationRequestedEvent to ProductService.
    //
    // Key Responsibilities:
    //   - Receive messages from RabbitMQ.
    //   - Deserialize JSON → OrderPlacedEvent (handled by BaseConsumer).
    //   - Create DI scope and resolve OrchestrationService.
    //   - Start the Saga orchestration (next step in workflow).
    public sealed class OrderPlacedConsumer : BaseConsumer<OrderPlacedEvent>
    {
        // Used to create new dependency injection scopes for each message.
        // Required because this consumer runs as a hosted background service,
        // and cannot directly use scoped services (like DbContext or business services).
        private readonly IServiceScopeFactory _scopeFactory;

        // --------------------------------------------------------
        // Constructor
        // Parameters:
        //   - channel: The RabbitMQ IModel channel used for consuming messages.
        //   - queueName: The queue name this consumer should listen to.
        //   - scopeFactory: Allows creation of scoped service providers per message.
        //   - logger: Structured logging support for visibility & diagnostics.
        //
        // Notes:
        //   - BaseConsumer<T> handles all the low-level message consumption logic:
        //       → queue subscription
        //       → JSON deserialization
        //       → automatic ACK/NACK handling
        // --------------------------------------------------------
        public OrderPlacedConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderPlacedConsumer> logger)
            : base(channel, queueName, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // --------------------------------------------------------
        // Method: HandleMessage
        // Description:
        //   This method is called automatically by the BaseConsumer
        //   whenever a new OrderPlacedEvent message is received from
        //   RabbitMQ and successfully deserialized.
        //
        // Workflow:
        //   1️. Create a new dependency injection scope.
        //   2️. Resolve IOrchestrationService from the scoped provider.
        //   3️. Call OnOrderPlacedAsync() to start the Saga orchestration flow.
        //
        // Message Flow Summary:
        //   → OrderService publishes "order.placed".
        //   → Orchestrator’s OrderPlacedConsumer receives it.
        //   → Calls OrchestrationService.OnOrderPlacedAsync().
        //   → Publishes "stock.reservation.requested" to ProductService.
        //
        // Error Handling:
        //   - If this method completes successfully → message is ACKed.
        //   - If an exception occurs → BaseConsumer will automatically NACK
        //     the message so it can be re-queued or moved to DLX (Dead Letter Queue).
        // --------------------------------------------------------
        protected override async Task HandleMessage(OrderPlacedEvent message)
        {
            // Create a DI scope for this message.
            // Each message gets a fresh scope to safely resolve scoped services
            // (e.g., DbContext, OrchestrationService) that should be disposed afterward.
            using var scope = _scopeFactory.CreateScope();

            // Resolve OrchestrationService from DI container.
            // This service contains the actual Saga orchestration logic
            // (publishing StockReservationRequestedEvent, caching order, etc.).
            var orchestrator = scope.ServiceProvider.GetRequiredService<IOrchestrationService>();

            // Pass the event to the orchestration logic.
            // This begins the distributed workflow by asking ProductService to reserve stock.
            await orchestrator.OnOrderPlacedAsync(message);

            // If no exceptions are thrown, BaseConsumer automatically ACKs the message,
            // marking it as successfully processed.
        }
    }
}
