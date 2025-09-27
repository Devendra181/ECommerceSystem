using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Contracts.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace ProductService.Infrastructure.Messaging
{
    public sealed class OrderPlacedConsumer : BackgroundService
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IModel _channel;
        private readonly RabbitMqOptions _opt;
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor: dependencies are injected by DI container.
        public OrderPlacedConsumer(
            ILogger<OrderPlacedConsumer> logger,
            IModel channel,
            IOptions<RabbitMqOptions> opt,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _channel = channel;
            _opt = opt.Value;
            _scopeFactory = scopeFactory;

            // Ensure exchange, queues, and bindings exist (idempotent).
            RabbitTopology.EnsureAll(_channel, _opt);
        }

        // BackgroundService requires ExecuteAsync.
        // This runs when the host starts and keeps listening until shutdown.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // QoS (Quality of Service) in RabbitMQ defines how many messages a consumer can receive and hold “in-flight” before acknowledging them.
            // Configure QoS: Allow max 10 unacknowledged messages per consumer at a time.
            //      prefetchSize = 0 → no size limit(we don’t restrict by bytes).
            //      prefetchCount = 10 → the consumer will receive at most 10 unacknowledged messages at once.
            //      global = false → this limit applies per consumer, not across the entire channel.
            _channel.BasicQos(0, 10, false);

            // Create an async RabbitMQ consumer to handle messages.
            var consumer = new AsyncEventingBasicConsumer(_channel);

            // Define what happens when a message is received.
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    // Deserialize the event payload into an OrderPlacedEvent object.
                    var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(ea.Body.Span);

                    // If deserialization fails, reject the message and don't requeue.
                    if (evt == null)
                    {
                        // This tells:
                        //      I can’t read this message.
                        //      Don’t keep it in the queue.
                        //      Send it to the Dead Letter Queue (DLQ).
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    // Create a new DI scope to resolve scoped services (e.g., DbContext, handler).
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IOrderPlacedHandler>();

                    // Pass the event to the handler (business logic to decrease stock).
                    await handler.HandleAsync(evt);

                    // Acknowledge the message (tell RabbitMQ it was processed successfully).
                    // This is you telling RabbitMQ: Hey, I got this message, processed it successfully, you can remove it from the queue now.
                    // Without this, RabbitMQ keeps the message “unacknowledged” and may redeliver it.
                    // Think of it like clicking “Mark as Done” after finishing a task.
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Log any errors and reject the message (move to DLQ if configured).
                    _logger.LogError(ex, "Error handling order.placed");

                    // This is you telling RabbitMQ: I couldn’t process this message. Don’t mark it as done.
                    // The three parts mean:
                    //      ea.DeliveryTag → which exact message you’re talking about.
                    //      false → only this single message(not multiple).
                    //      false → don’t put it back in the same queue(send it to DLQ if configured).
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            // Start consuming messages from the configured queue (ProductOrderPlacedQueue).
            // autoAck = false means manual acknowledgment (we control when to Ack/Nack).

            // This is you telling RabbitMQ: Send me messages, but don’t auto-mark them as done. I’ll tell you myself when I’m finished.
            // autoAck = false → you take control.
            // This is why you need BasicAck above.
            // If processing fails, you can send a Nack instead, and RabbitMQ can retry or send to DLQ.
            _channel.BasicConsume(
                queue: _opt.ProductOrderPlacedQueue,
                autoAck: false,
                consumer: consumer);

            // Return completed task since this runs in the background forever.
            return Task.CompletedTask;
        }
    }
}
