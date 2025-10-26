using Messaging.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Messaging.Consumers;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging.Extensions
{
    // This static extension class adds a clean, reusable method to
    // register all RabbitMQ consumers required by the NotificationService.
    //
    // Instead of registering each consumer manually in Program.cs,
    // this keeps the startup configuration organized and centralized.
    //
    // Each consumer is hosted as a background service (IHostedService)
    // and runs continuously, listening for messages from specific queues.
    public static class RabbitMqConsumerExtensions
    {
        // Registers both OrderConfirmedConsumer and OrderCancelledConsumer
        // as hosted background services in the application's dependency injection container.
        //
        // Each hosted service:
        //  - Opens a RabbitMQ channel connection.
        //  - Listens to its respective queue.
        //  - Processes messages via BaseConsumer<T>.
        public static IServiceCollection AddNotificationConsumers(this IServiceCollection services)
        {
            // ------------------------------------------------------------------
            // Register OrderConfirmedConsumer
            // ------------------------------------------------------------------
            services.AddHostedService(sp =>
            {
                // Retrieve RabbitMQ channel (IModel) from DI.
                // This channel is used to receive and acknowledge messages.
                var channel = sp.GetRequiredService<IModel>();

                // Get RabbitMQ configuration (exchange, queue names, etc.)
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                // Create a scope factory — allows consumers to create new
                // DI scopes per message, ensuring clean service lifetimes.
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                // Get a logger instance for structured logs per consumer.
                var logger = sp.GetRequiredService<ILogger<OrderConfirmedConsumer>>();

                // Create and return a new hosted background consumer
                // for handling "OrderConfirmedEvent" messages.
                return new OrderConfirmedConsumer(
                    channel,
                    options.QNotificationOrderConfirmed, // Queue name from RabbitMqOptions
                    scopeFactory,
                    logger);
            });

            // ------------------------------------------------------------------
            // Register OrderCancelledConsumer
            // ------------------------------------------------------------------
            services.AddHostedService(sp =>
            {
                // Each consumer uses the same pattern:
                // Resolve dependencies from DI for this background process.
                var channel = sp.GetRequiredService<IModel>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<OrderCancelledConsumer>>();

                // Create a hosted background consumer for "OrderCancelledEvent".
                return new OrderCancelledConsumer(
                    channel,
                    options.QNotificationOrderCancelled, // Queue for order cancellation notifications
                    scopeFactory,
                    logger);
            });

            // ------------------------------------------------------------------
            // Return IServiceCollection so that this method can be chained fluently
            // in Program.cs during application startup.
            // Example usage:
            //     builder.Services.AddNotificationConsumers();
            // ------------------------------------------------------------------
            return services;
        }
    }
}

