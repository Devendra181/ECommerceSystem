using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductService.Contracts.Messaging;
using ProductService.Contracts.Models;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging.Consumers
{
    // RabbitMQ Consumer responsible for handling "StockReservationRequestedEvent" messages.
    // 
    //    This class listens to the queue bound to the routing key "stock.reservation.requested"
    //    (declared in RabbitTopology) and is triggered whenever the OrchestratorService
    //    requests the ProductService to reserve stock for an order.
    // 
    //    Once a message arrives:
    //     1. It creates a new DI scope (per message).
    //     2. Calls IStockReserveService to perform business logic (check and deduct stock).
    //     3. Publishes either StockReservedCompletedEvent or StockReservationFailedEvent
    //        depending on the result.
    public sealed class StockReserveConsumer : BaseConsumer<StockReservationRequestedEvent>
    {
        // Factory used to create scoped service providers for each message
        // (important because DbContext and other services are registered as Scoped)
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor — receives shared RabbitMQ channel, queue name, DI scope factory, and logger.
        // The base constructor (BaseConsumer) subscribes this consumer to the queue.
        public StockReserveConsumer(
            IModel channel,                       // RabbitMQ channel for consuming messages
            string queueName,                     // Queue this consumer listens to
            IServiceScopeFactory scopeFactory,    // Used to resolve scoped services per message
            ILogger<StockReserveConsumer> logger  // For logging processing steps/errors
        ) : base(channel, queueName, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // Handles the incoming StockReservationRequestedEvent message.
        //    Executes inside the background worker defined by BaseConsumer.
        //    This is where the main stock reservation business logic is triggered.
        protected override async Task HandleMessage(StockReservationRequestedEvent message)
        {
            // STEP 1: Create a per-message dependency injection scope 
            // Ensures we get fresh scoped instances (e.g., DbContext, repositories, services)
            using var scope = _scopeFactory.CreateScope();

            // Resolve required application-level services from DI
            var inventory = scope.ServiceProvider.GetRequiredService<IStockReserveService>();       // Handles stock check & reservation
            var publisher = scope.ServiceProvider.GetRequiredService<IStockReserveEventPublisher>(); // Publishes outcome back to Orchestrator

            // STEP 2: Execute the stock reservation logic
            // Calls the core application service to check stock and attempt reservation
            StockReservationResult result = await inventory.StockReserveAsync(message);

            // STEP 3: Based on result, publish appropriate outcome event
            if (result.Success)
            {
                // Success path → build StockReservedCompletedEvent
                var success = new StockReservedCompletedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    UserId = message.UserId,
                    Items = message.Items // same items confirmed as reserved
                };

                // Publish success event → consumed by OrchestratorService (StockReservedConsumer)
                await publisher.PublishStockReservedCompletedAsync(success);
            }
            else
            {
                // Failure path → build StockReservationFailedEvent
                var fail = new StockReservationFailedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    UserId = message.UserId,
                    Reason = result.FailureReason ?? "Insufficient stock",
                    FailedItems = result.FailedItems
                };

                // Publish failure event → consumed by OrchestratorService (StockReservationFailedConsumer)
                await publisher.PublishStockReservationFailedAsync(fail);
            }
        }
    }
}
