using Messaging.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.Infrastructure.Messaging.Consumers;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging.Extensions
{
    // Purpose:
    //   Provides an extension method for registering the
    //   OrderCancelledConsumer as a hosted background service.
    //
    //   This helps modularize RabbitMQ consumer registration logic
    //   and keeps Program.cs clean and maintainable.
    //
    //   In the Saga Orchestration flow:
    //     - The OrchestratorService publishes "OrderCancelledEvent"
    //       when a distributed transaction fails (e.g., stock unavailable).
    //     - This consumer listens for that message and triggers
    //       compensation logic inside OrderService.
    public static class RabbitMqConsumerExtensions
    {
        // Method: AddOrderCancelledConsumer
        // Description:
        //   Registers the OrderCancelledConsumer as a Hosted Service.
        //   Hosted services run continuously in the background and are
        //   ideal for long-running message listeners like RabbitMQ consumers.
        //
        // Parameters:
        //   services → IServiceCollection used for dependency injection.
        //
        // Return:
        //   IServiceCollection → Allows method chaining after registration.
        //
        // Notes:
        //   - This pattern helps you easily add more consumers in the future
        //     (e.g., AddOrderConfirmedConsumer, AddPaymentFailedConsumer, etc.)
        //   - Keeps Program.cs less cluttered and more modular.
        public static IServiceCollection AddOrderCancelledConsumer(this IServiceCollection services)
        {
            // Register the OrderCancelledConsumer as a background service.
            // The AddHostedService method automatically manages its lifecycle:
            //   - Starts when the application starts.
            //   - Stops when the app shuts down.
            services.AddHostedService(sp =>
            {
                // Resolve all required dependencies from the DI container.

                // RabbitMQ channel (IModel) used for queue subscription and message consumption.
                var channel = sp.GetRequiredService<IModel>();

                // Scope factory used to create DI scopes for resolving scoped services
                // (like DbContext) inside the consumer while handling each message.
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                // Logger for the OrderCancelledConsumer to log message processing info and errors.
                var logger = sp.GetRequiredService<ILogger<OrderCancelledConsumer>>();

                // Retrieve RabbitMQ configuration (exchange, queues, routing keys) from appsettings.json.
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                // Create and return an instance of OrderCancelledConsumer.
                // Parameters:
                //   - channel: shared RabbitMQ communication channel.
                //   - options.QOrderCompensationCancelled: queue name for
                //     compensation messages ("order.compensation_cancelled").
                //   - scopeFactory: used to create new scopes for message processing.
                //   - logger: logs message handling operations.
                //
                // The consumer automatically subscribes to the queue when started.
                return new OrderCancelledConsumer(
                    channel,
                    options.QOrderCompensationCancelled,  // Queue name from RabbitMqOptions
                    scopeFactory,
                    logger
                );
            });

            // Return IServiceCollection for method chaining.
            return services;
        }
    }
}

