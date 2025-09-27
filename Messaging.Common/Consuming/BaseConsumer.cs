using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Common.Consuming
{
    // Reusable consumer base (HostedService).
    // - Subscribes to a queue
    // - Deserializes messages into <T>
    // - Calls HandleMessage(T, correlationId)
    // - ACKs on success, NACKs on failed
    // T: The type of message being consumed (DTO/Event).
    public abstract class BaseConsumer<T> : BackgroundService
    {
        // The RabbitMQ channel (IModel) this consumer will use.
        private readonly IModel _channel;

        // The name of the queue this consumer listens to.
        private readonly string _queue;

        private readonly ILogger _logger;

        // Constructor: accepts the channel and queue name as inputs.
        protected BaseConsumer(IModel channel, string queueName, ILogger logger)
        {
            _channel = channel;
            _queue = queueName;
            _logger = logger;
        }

        // Starts consuming messages from the configured queue.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create an async consumer for this channel.
            var consumer = new AsyncEventingBasicConsumer(_channel);

            // consumer is an instance of AsyncEventingBasicConsumer.
            // .Received is an event that RabbitMQ raises whenever a new message arrives in the queue.
            // The += operator means: subscribe to this event with the following handler (a lambda function).
            // So this says: "When a message arrives, run this block of code."

            consumer.Received += async (model, ea) =>
            {
                // This is a lambda expression (an inline function) with two parameters:
                // model → sender (the consumer object, usually ignored).
                // ea → an instance of BasicDeliverEventArgs (the delivery details).
                // The ea object is very important — it contains:
                // ea.Body → the actual message payload(in bytes).
                // ea.BasicProperties → metadata(like CorrelationId, headers, delivery mode).
                // ea.DeliveryTag → a unique number RabbitMQ assigns to this message(used for ACK / NACK).
                // ea.RoutingKey, ea.Exchange, etc.

                try
                {
                    // Convert the message body (byte array) into a UTF-8 string.
                    var body = Encoding.UTF8.GetString(ea.Body.Span);

                    // Deserialize the JSON string into the expected type T.
                    var msg = JsonSerializer.Deserialize<T>(body)!;

                    // CorrelationId is useful for tracing across services
                    var correlationId = ea.BasicProperties?.CorrelationId ?? string.Empty;

                    // Call the abstract handler method for actual business logic.
                    await HandleMessage(msg!, correlationId);

                    // If no exception occurs → acknowledge the message (mark as processed).
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    // ACK = "Acknowledgement".
                    // Informs RabbitMQ: I successfully processed this message, you can remove it from the queue.
                    // Uses the unique DeliveryTag from ea.
                    // multiple: false → only ACK this single message(not multiple at once).
                    // Without this, RabbitMQ will think the message was not processed and will redeliver it.
                }
                catch (Exception ex)
                {
                    // If something goes wrong → log the error.
                    _logger.LogError($"Message: {ex.Message}: Complete Message: {ex.ToString()}");
                    // Negative Acknowledgement → message goes to DLQ (if DLX is configured).
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);

                    // NACK = "Negative Acknowledgement".
                    // Tells RabbitMQ: I could not process this message.
                    // requeue: false → don’t put it back in the same queue → instead send it to the Dead Letter Exchange(DLX) if configured.
                    // If DLX isn’t configured → message is discarded.
                }
            };

            // Prefetch=1 to process messages sequentially per consumer instance
            _channel.BasicQos(0, 1, global: false);

            // Begin consuming messages from the queue.
            // Start delivering messages from this queue to my consumer,
            // and I will manually confirm (ACK/NACK) each one.
            _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);

            _logger.LogInformation("Started consumer for queue: {Queue}", _queue);
            return Task.CompletedTask;
        }

        // Implement business logic here.
        // Abstract method for handling a message.
        // Must be implemented in derived consumers (e.g., PaymentConsumer, InventoryConsumer).
        protected abstract Task HandleMessage(T message, string correlationId);
    }
}
