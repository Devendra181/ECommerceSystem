using Microsoft.Extensions.DependencyInjection;
using Messaging.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ProductService.Infrastructure.Messaging.Consumers;

namespace ProductService.Infrastructure.Messaging.Extensions
{
    // Extension class that cleanly registers the StockReserveConsumer as a hosted background service.
    // 
    // This allows the ProductService to automatically start listening for
    // "stock.reservation.requested" messages from RabbitMQ as soon as the app runs.
    // 
    // The extension pattern keeps Program.cs clean by encapsulating consumer registration logic.
    public static class RabbitMqConsumerExtensions
    {
        // Registers the StockReserveConsumer as a hosted service inside the dependency injection (DI) container.
        // 
        // This method is typically called inside Program.cs like:
        // builder.Services.AddStockReserveConsumer();
        // 
        // The hosted service will continuously consume messages from the queue defined
        // in RabbitMqOptions.QProductStockReservationRequested.
        public static IServiceCollection AddStockReserveConsumer(this IServiceCollection services)
        {
            // Register the consumer as a HostedService → background worker managed by ASP.NET Core.
            services.AddHostedService(sp =>
            {
                // Resolve RabbitMQ Channel (IModel)
                // This represents the live connection to RabbitMQ for consuming messages.
                var channel = sp.GetRequiredService<IModel>();

                // Resolve Scope Factory
                // Required to create a new service scope for each message processed.
                // (Important because DbContext and repositories are scoped services.)
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                // Resolve Logger
                // Injects a typed logger for the StockReserveConsumer to log message processing, errors, etc.
                var logger = sp.GetRequiredService<ILogger<StockReserveConsumer>>();

                // Resolve RabbitMQ Configuration
                // Retrieves queue and exchange names defined in appsettings.json (under "RabbitMq" section).
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                // Create and return StockReserveConsumer instance
                // Binds consumer to the correct queue (e.g., "product.stock_reservation_requested")
                // and injects all required dependencies.
                return new StockReserveConsumer(
                    channel,                                     // RabbitMQ channel
                    options.QProductStockReservationRequested,   // Queue name this consumer listens to
                    scopeFactory,                                // Factory for per-message DI scopes
                    logger                                       // Logging support
                );
            });

            // Return the IServiceCollection so this can be chained fluently in Program.cs
            return services;
        }
    }
}
