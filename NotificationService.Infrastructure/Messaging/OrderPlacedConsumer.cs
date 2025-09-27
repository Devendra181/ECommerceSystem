using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Contracts.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public sealed class OrderPlacedConsumer : BackgroundService
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;   // Logging for diagnostics.
        private readonly IModel _channel;                        // RabbitMQ channel to consume messages.
        private readonly RabbitMqOptions _options;               // RabbitMQ configuration values (queues, exchange, etc.).
        private readonly IServiceScopeFactory _scopeFactory;     // Used to resolve scoped dependencies inside a singleton consumer.

        // Constructor: dependencies are injected by DI.
        public OrderPlacedConsumer(
            ILogger<OrderPlacedConsumer> logger,                 // Logging instance
            IModel channel,                                      // RabbitMQ channel
            IOptions<RabbitMqOptions> options,                   // Injected RabbitMQ options from appsettings.json
            IServiceScopeFactory scopeFactory)                   // Scope factory for resolving scoped services (like DbContext, handlers)
        {
            _logger = logger;
            _channel = channel;
            _options = options.Value;                            // Extract actual RabbitMQ settings
            _scopeFactory = scopeFactory;

            // Ensure exchange, queues, and bindings exist before consuming.
            RabbitTopology.EnsureAll(_channel, _options);
        }

        // BackgroundService entry point → runs when the host starts.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure QoS: max 10 unacknowledged messages per consumer.
            // Prevents flooding one consumer with too many messages at once.
            _channel.BasicQos(0, 10, false);

            // Create an async consumer for RabbitMQ
            var consumer = new AsyncEventingBasicConsumer(_channel);

            // Event handler: triggered when a new message is received
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    // Deserialize the raw message body into an OrderPlacedEvent.
                    // PropertyNameCaseInsensitive = true allows both camelCase and PascalCase JSON.
                    var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(ea.Body.Span, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // If deserialization failed, reject the message and send to DLQ.
                    if (evt == null)
                    {
                        _logger.LogWarning("Received null or invalid OrderPlacedEvent.");
                        _channel.BasicNack(ea.DeliveryTag, false, false); // NACK → reject without requeue
                        return;
                    }

                    // Create a new DI scope so we can resolve scoped services (like IOrderPlacedHandler).
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IOrderPlacedHandler>();

                    // Pass the event to the handler (business logic: create notification entry).
                    await handler.HandleAsync(evt);

                    // Acknowledge the message → remove from queue after successful processing.
                    _channel.BasicAck(ea.DeliveryTag, false);

                    // Log success with the OrderId for traceability.
                    _logger.LogInformation("Processed OrderPlacedEvent for Order {OrderId}", evt.OrderId);
                }
                catch (Exception ex)
                {
                    // Log any failure, reject the message (send to DLQ).
                    _logger.LogError(ex, "Failed to process OrderPlacedEvent");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            // Start consuming messages from the Notification queue.
            // autoAck = false → we manually Ack/Nack messages after processing.
            _channel.BasicConsume(
                queue: _options.NotificationOrderPlacedQueue,
                autoAck: false,
                consumer: consumer);

            // Return completed task → consumer runs in background indefinitely.
            return Task.CompletedTask;
        }
    }
}
