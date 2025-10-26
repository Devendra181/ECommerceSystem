using Messaging.Common.Consuming;
using Messaging.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Messaging;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    //    This consumer listens for the "OrderConfirmedEvent" messages
    //    that are published by the OrchestratorService once an order
    //    has successfully completed the Saga flow (i.e., stock reserved, order confirmed).
    //    
    //    It belongs to the NotificationService microservice and acts as a bridge
    //    between RabbitMQ (messaging infrastructure) and the application logic
    //    that actually sends notifications to customers.
    public sealed class OrderConfirmedConsumer : BaseConsumer<OrderConfirmedEvent>
    {
        // Used to create a new DI (Dependency Injection) scope for each message.
        // This ensures scoped services (like DbContext or business services)
        // are properly created and disposed per message.
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor initializes the base consumer with:
        //  - RabbitMQ channel (IModel): to receive and acknowledge messages.
        //  - Queue name: the specific queue this consumer listens to.
        //  - ILogger: for structured logging of message processing.
        public OrderConfirmedConsumer(
            IModel channel,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderConfirmedConsumer> logger)
            : base(channel, queueName, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // Handles the message logic for "OrderConfirmedEvent".
        // This method is automatically triggered whenever a new message
        // arrives in the configured RabbitMQ queue.
        protected override async Task HandleMessage(OrderConfirmedEvent message)
        {
            Console.WriteLine($"NotificationService [Consumer] → OrderConfirmedConsumer for OrderId={message.OrderId}");

            // STEP 1️: Create a new service scope.
            // This ensures each message gets isolated service instances,
            // avoiding concurrency issues or DbContext lifetime conflicts.
            using var scope = _scopeFactory.CreateScope();

            // STEP 2️: Resolve the application-layer handler from DI.
            // The handler (NotificationServiceHandler) contains the actual
            // business logic for building and sending the notification.
            var app = scope.ServiceProvider.GetRequiredService<INotificationServiceHandler>();

            // STEP 3️: Delegate processing to the application layer.
            // The handler will prepare the notification template data,
            // create a notification entry, and trigger email/SMS delivery.
            await app.HandleOrderConfirmedAsync(message);
        }
    }
}

