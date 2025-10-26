using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Messaging;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    // This class listens for "OrderCancelledEvent" messages
    // that are published by the OrchestratorService.
    //
    // The Orchestrator publishes this event when an order fails
    // during the Saga transaction — for example, when ProductService
    // reports insufficient stock or another step in the workflow fails.
    //
    // Once this consumer receives the message, it triggers the
    // NotificationServiceHandler in the Application layer to send
    // a cancellation notification (via email/SMS) to the customer.
    public sealed class OrderCancelledConsumer : BaseConsumer<OrderCancelledEvent>
    {
        // Used to create a new DI (Dependency Injection) scope per message.
        // Each message will execute in its own scope so that scoped services
        // (like DbContext or other injected dependencies) are safely created
        // and disposed independently.
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor: Injects all necessary dependencies for consuming messages.
        // - channel: The RabbitMQ channel that connects to the queue.
        // - queueName: The name of the queue this consumer listens to.
        // - scopeFactory: Used to create scoped service providers per message.
        // - logger: Logs message lifecycle events for visibility and debugging.
        public OrderCancelledConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCancelledConsumer> logger)
            : base(channel, queueName, logger) // Initialize the base consumer.
        {
            _scopeFactory = scopeFactory;
        }

        // Core logic for processing a single message from RabbitMQ.
        // This method is automatically invoked when a new
        // OrderCancelledEvent arrives in the queue.
        protected override async Task HandleMessage(OrderCancelledEvent message)
        {
            Console.WriteLine($"NotificationService [Consumer] → OrderCancelledConsumer for OrderId={message.OrderId}");

            // STEP 1️: Create a new dependency injection scope.
            // Each message gets its own scope to ensure thread safety
            // and proper disposal of services like DbContext.
            using var scope = _scopeFactory.CreateScope();

            // STEP 2️: Resolve the application-level handler.
            // The handler (NotificationServiceHandler) contains
            // the actual business logic for building and sending
            // a "cancellation" notification to the user.
            var app = scope.ServiceProvider.GetRequiredService<INotificationServiceHandler>();

            // STEP 3️: Delegate message processing to the handler.
            // The handler constructs a structured notification request,
            // fills the appropriate template data (OrderNumber, Reason, etc.),
            // and calls the NotificationService to persist and dispatch it.
            await app.HandleOrderCancelledAsync(message);

            // Once this completes successfully, the BaseConsumer
            // will automatically ACK the message to RabbitMQ,
            // signaling that it was processed without error.
        }
    }
}

