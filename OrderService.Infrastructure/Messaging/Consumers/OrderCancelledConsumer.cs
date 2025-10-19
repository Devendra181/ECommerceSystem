using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Contracts.Messaging;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    // Purpose:
    //   Listens for "OrderCancelledEvent" messages published by the
    //   OrchestratorService during the Saga compensation process.

    //   When a downstream service (like ProductService or PaymentService) fails,
    //   the OrchestratorService decides to cancel the order and publishes an OrderCancelledEvent.
    //   This consumer receives that message and triggers compensation logic in the OrderService
    //   (e.g., updating the order’s status in the database to "Cancelled").

    // Notes:
    //   - Inherits from BaseConsumer<T>, which provides core message
    //     consumption and deserialization behavior.
    //   - Uses dependency injection to create scopes and resolve
    //     services safely (e.g., DbContext via Application layer handler).
    public sealed class OrderCancelledConsumer : BaseConsumer<OrderCancelledEvent>
    {
        // Field: _scopeFactory
        // Description:
        //   The IServiceScopeFactory is used to create a new DI scope for every incoming message.
        //   Since this consumer runs as a BackgroundService (singleton),
        //   we cannot inject scoped services (like DbContext) directly.
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor:
        // Parameters:
        //   - channel:     The RabbitMQ channel (IModel) used to subscribe
        //                  to the appropriate queue and consume messages.

        //   - queueName:   The specific queue name this consumer listens on,
        //                  typically configured in RabbitMqOptions (e.g.,
        //                  "order.compensation_cancelled").

        //   - scopeFactory: Used to create per-message DI scopes for resolving
        //                   scoped services like repositories or DbContexts.

        //   - logger:      Used to record information or errors during message
        //                  processing for observability and debugging.

        // Notes:
        //   - The base class (BaseConsumer<OrderCancelledEvent>) handles
        //     low-level RabbitMQ consumer setup and message acknowledgment.
        public OrderCancelledConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCancelledConsumer> logger)
            : base(channel, queueName, logger) // Calls BaseConsumer constructor
        {
            _scopeFactory = scopeFactory;
        }

        // Method: HandleMessage
        // Description:
        //   Executes when a new "OrderCancelledEvent" message is received from RabbitMQ.
        //   This method delegates the actual business logic
        //   (updating the order status to Cancelled) to the Application layer.

        // Flow:
        //   1. Create a new DI scope for safe usage of scoped services.
        //   2. Resolve IOrderCancelledHandler from the scoped service provider.
        //   3. Pass the deserialized event and correlationId to the handler.
        //   4. Handler updates the database (order → Cancelled).

        // Parameters:
        //   - message:       The OrderCancelledEvent payload received from the queue.
        protected override async Task HandleMessage(OrderCancelledEvent message)
        {
            // Create a new dependency injection scope for this message.
            // This ensures that scoped dependencies (like DbContext or repositories)
            // are properly managed and disposed after the message is processed.
            using var scope = _scopeFactory.CreateScope();

            // Resolve the application-layer handler responsible for executing
            // the compensation logic (marking the order as Cancelled).
            var handler = scope.ServiceProvider.GetRequiredService<IOrderCancelledHandler>();

            // Invoke the handler method asynchronously.
            // This is where the business logic (database update, audit log, etc.)
            // is executed to finalize the compensation.
            await handler.HandleAsync(message);
        }
    }
}

