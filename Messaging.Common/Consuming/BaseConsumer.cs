using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Common.Consuming
{
    // BaseConsumer is an abstract, reusable class that handles all the
    // repeated logic required for consuming messages from RabbitMQ queues.

    // It integrates with ASP.NET Core's background service model, which allows
    // consumers to start automatically when the host application runs.

    // Derived classes only need to implement the HandleMessage() method
    // to define their own business logic for each message.

    // T: The message type expected from the queue.
    public abstract class BaseConsumer<T> : BackgroundService
    {
        // --------------------------------------------------------------------
        // Private Fields
        // --------------------------------------------------------------------

        // The RabbitMQ channel (IModel) used for consuming messages.
        private readonly IModel _channel;

        // The name of the queue that this consumer will subscribe to.
        private readonly string _queue;

        // Logger instance used to record informational or error messages.
        // Helps track message consumption, errors, and debugging.
        private readonly ILogger _logger;

        // --------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------
        // Initializes the BaseConsumer with a specific RabbitMQ channel, queue name, and logger instance.
        // Parameters:
        //      channel: The RabbitMQ channel used for consuming messages.
        //      queueName: The name of the queue this consumer listens to.
        //      logger: The logging service for tracking events and errors.
        protected BaseConsumer(IModel channel, string queueName, ILogger logger)
        {
            _channel = channel;
            _queue = queueName;
            _logger = logger;
        }

        // --------------------------------------------------------------------
        // ExecuteAsync (Entry Point)
        // --------------------------------------------------------------------
        // The main entry point for the background service.
        // Called automatically when the BackgroundService starts.
        // This method sets up an asynchronous RabbitMQ consumer that listens to the specified queue.
        // It automatically deserializes messages, calls the handler, and manages acknowledgments.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create an asynchronous event-based consumer to receive messages from the queue.
            // AsyncEventingBasicConsumer allows async processing without blocking threads.
            var consumer = new AsyncEventingBasicConsumer(_channel);

            // ----------------------------------------------------------------
            // Step 1: Configure Quality of Service (QoS)
            // ----------------------------------------------------------------
            // Configures how many messages the consumer can prefetch (receive) before ACKing previous ones.
            // Parameters:
            //   - prefetchSize: 0 
            //         → This disables size-based limiting (we don’t limit by message size in bytes).
            //           RabbitMQ will ignore message size and only use message count.
            //   - prefetchCount: 1 
            //         → This tells RabbitMQ to deliver only one unacknowledged message at a time to this consumer.
            //           The consumer must ACK before the next message is sent.
            //           Ensures sequential and controlled message processing
            //   - global: false 
            //         → This setting applies only to this channel/consumer instance.
            //           If set to true, it would apply globally to all consumers on the same channel.
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // The Received event is triggered every time a new message arrives in the subscribed queue.
            // It provides two parameters:
            //
            // 1. '_' → This represents the sender object (the source that raised the event).
            //    The underscore (_) means "we don’t need this variable" — it’s ignored intentionally.
            //
            // 2. 'ea' → This is an instance of BasicDeliverEventArgs.
            //    It contains all the information about the received message, including:
            //       - ea.Body → The raw message payload (byte array)
            //       - ea.DeliveryTag → Unique identifier for the message (used for ACK/NACK)
            //       - ea.BasicProperties → Metadata (like CorrelationId, Headers, ContentType, etc.)
            //       - ea.RoutingKey → The routing key used to deliver the message
            consumer.Received += async (_, ea) =>
            {
                // consumer is an instance of AsyncEventingBasicConsumer.
                // .Received is an event that RabbitMQ raises whenever a new message arrives in the queue.
                // The += operator means: subscribe to this event with the following handler (a lambda function).
                // So this says: "When a message arrives, run this block of code."
                try
                {
                    // ------------------------------------------------------------
                    // Step 2: Deserialize the Message
                    // ------------------------------------------------------------
                    // The message payload (body) arrives as a byte array.
                    // Convert the incoming byte array into a json string 
                    var json = Encoding.UTF8.GetString(ea.Body.Span);

                    // Then, deserialize the JSON string into the target message type (T).
                    // The "!" indicates that we're confident the deserialization won't return null.
                    var msg = JsonSerializer.Deserialize<T>(json)!;

                    // ------------------------------------------------------------
                    // Step 3: Invoke Business Logic
                    // ------------------------------------------------------------
                    // Delegate the business logic to the derived class implementation.
                    // Each derived class defines what to do when a message is received.
                    // We are passing the target message type and correlationId
                    await HandleMessage(msg);

                    // ------------------------------------------------------------
                    // Step 4: Acknowledge Success
                    // ------------------------------------------------------------
                    // Send an Acknowledges signal to RabbitMQ to confirm successful processing.
                    // This tells the broker that this message has been processed successfully
                    // and can be removed from the queue.

                    // Parameters:
                    //   - ea.DeliveryTag : A unique number assigned by RabbitMQ to every delivered message.
                    //                      It helps the broker identify which specific message is being ACKed.
                    //   - multiple: false: Means "acknowledge only this single message" (not multiple).
                    //                      If it is true, RabbitMQ would ACK all previous unacknowledged messages
                    //                      up to and including this tag. (useful for batch acking)
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    // ------------------------------------------------------------
                    // Step 5: Handle Exceptions
                    // ------------------------------------------------------------
                    // If anything goes wrong during message handling,
                    // log the error details for troubleshooting.
                    _logger.LogError($"[Consumer Error] {ex.Message}. StackTrace: {ex}");

                    // Send a NACK (Negative Acknowledgment) to RabbitMQ.
                    // This tells RabbitMQ that processing failed.
                    // Setting requeue: true means the message will go back to the queue
                    // for another attempt (or be sent to the DLQ if retries exceed limits).
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            // ----------------------------------------------------------
            // Step 6: Start Consuming Messages
            // ----------------------------------------------------------
            // Start consuming messages from the specified queue.
            // This method registers a consumer to listen for incoming messages and handle them asynchronously.
            // Parameters:
            //  - queue: _queue → The name of the queue that this consumer will subscribe to.
            //                    This is the same queue name that was passed into the constructor
            //                    when this BaseConsumer instance was created.

            //  - autoAck: false → "Automatic Acknowledgment" is disabled.
            //                     This means the consumer must manually acknowledge (ACK) each message
            //                     after successful processing using `_ch.BasicAck(...)`.
            //                     If set to true, RabbitMQ would auto-acknowledge messages as soon as
            //                     they are delivered — but that’s risky because if processing fails,
            //                     the message would be lost forever.

            //  - consumer: consumer → The instance of AsyncEventingBasicConsumer that listens for messages.
            //                         It handles message delivery events (`consumer.Received`) asynchronously.

            // In short:
            //  - We’re subscribing to a specific queue (_queue),
            //  - We’re handling messages manually (autoAck = false),
            //  - We’re using our AsyncEventingBasicConsumer instance to process incoming messages.
            _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);

            // Log that the consumer has started successfully.
            _logger.LogInformation("Consumer started and listening on queue: {Queue}", _queue);

            // BackgroundService requires returning a Task — no continuous loop needed,
            // because message handling is event-driven.
            return Task.CompletedTask;
        }

        // --------------------------------------------------------------------
        // Abstract Method: Message Handling Logic
        // --------------------------------------------------------------------
        // Each subclass must override this method to define its message-handling logic.
        // Must be implemented in derived consumers (e.g., PaymentConsumer, InventoryConsumer).
        // This is where your actual business logic for processing messages goes.
        //      BaseConsumer handles all the RabbitMQ plumbing; 
        //      Subclasses focus purely on business logic.

        // Parameters:
        //      message: The deserialized message object of type T.
        protected abstract Task HandleMessage(T message);
    }
}
