using Messaging.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchestratorService.Infrastructure.Messaging.Consumers;
using RabbitMQ.Client;
namespace OrchestratorService.Infrastructure.Messaging.Extensions
{
    // Extension methods for registering RabbitMQ consumers
    // (background hosted services) for the OrchestratorService.
    public static class RabbitMqConsumerExtensions
    {
        // Registers all RabbitMQ consumers required by the Orchestrator.
        // Each consumer runs as a hosted background service and listens
        // to a specific queue for incoming events in the Saga workflow.
        public static IServiceCollection AddOrchestratorConsumers(this IServiceCollection services)
        {
            // Order Placed Consumer
            // - Listens to queue: orchestrator.order_placed
            // - Triggered when OrderService publishes OrderPlacedEvent
            // - Starts the Saga flow by asking ProductService to reserve stock
            services.AddHostedService(sp =>
            {
                var channel = sp.GetRequiredService<IModel>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<OrderPlacedConsumer>>();
                return new OrderPlacedConsumer(
                    channel,
                    options.QOrchestratorOrderPlaced,
                    scopeFactory,
                    logger);
            });

            // Stock Reserved Consumer
            // - Listens to queue: orchestrator.stock_reserved
            // - Triggered when ProductService successfully reserves stock
            // - Publishes OrderConfirmedEvent (success path of the Saga)
            services.AddHostedService(sp =>
            {
                var channel = sp.GetRequiredService<IModel>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<StockReservedConsumer>>();
                return new StockReservedConsumer(
                    channel,
                    options.QOrchestratorStockReserved,
                    scopeFactory,
                    logger);
            });

            // Stock Reservation Failed Consumer
            // - Listens to queue: orchestrator.stock_failed
            // - Triggered when ProductService fails to reserve stock
            // - Publishes OrderCancelledEvent (compensation path)
            services.AddHostedService(sp =>
            {
                var channel = sp.GetRequiredService<IModel>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<StockReservationFailedConsumer>>();
                return new StockReservationFailedConsumer(
                    channel,
                    options.QOrchestratorStockFailed,
                    scopeFactory,
                    logger);
            });

            return services;
        }
    }
}
