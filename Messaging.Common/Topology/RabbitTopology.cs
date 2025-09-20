using Messaging.Common.Options;
using RabbitMQ.Client;

namespace Messaging.Common.Topology
{
    public static class RabbitTopology
    {
        public static void EnsureAll(IModel channel, RabbitMqOptions rabbitMqOptions)
        {
            // Declare the main exchange (topic-based)
            //    - Durable: survives broker restarts
            //    - AutoDelete: false means it won't disappear when unused
            //    - Type: Topic exchange routes messages based on pattern matching
            channel.ExchangeDeclare(
                exchange: rabbitMqOptions.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare the Dead Letter Exchange (DLX) if configured
            //    - Used for failed/rejected messages (safety net)
            if (!string.IsNullOrWhiteSpace(rabbitMqOptions.DlxExchangeName))
            {
                channel.ExchangeDeclare(
                    exchange: rabbitMqOptions.DlxExchangeName!,
                    type: ExchangeType.Fanout,   // Fanout: send dead letters to all bound queues
                    durable: true,
                    autoDelete: false);

                // Declare Dead Letter Queue if provided
                if (!string.IsNullOrWhiteSpace(rabbitMqOptions.DlxQueueName))
                {
                    channel.QueueDeclare(
                        queue: rabbitMqOptions.DlxQueueName!,
                        durable: true,      // survive broker restarts
                        exclusive: false,   // can be consumed by multiple consumers
                        autoDelete: false,  // not auto-deleted when last consumer disconnects
                        arguments: null);

                    // Bind DLQ to DLX (routingKey irrelevant for fanout exchange)
                    channel.QueueBind(rabbitMqOptions.DlxQueueName, rabbitMqOptions.DlxExchangeName!, routingKey: "");
                }
            }

            // Common queue arguments (applied to business queues)
            //    - Add DLX binding if one exists, so rejected messages are routed safely
            var args = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(rabbitMqOptions.DlxExchangeName))
                args["x-dead-letter-exchange"] = rabbitMqOptions.DlxExchangeName;
            //args["x-message-ttl"] = 10000;
            //args["x-max-length"] = 100;

            // Declare ProductService queue (listens for order.placed events)
            channel.QueueDeclare(
                queue: rabbitMqOptions.ProductOrderPlacedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args); // attach DLX args if available

            // Declare NotificationService queue (listens for order.placed events)
            channel.QueueDeclare(
                queue: rabbitMqOptions.NotificationOrderPlacedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args); // attach DLX args if available

            // Bind queues to the main exchange with the routing key "order.placed"
            //    - Any publisher sending to exchange "ecommerce.topic" with routingKey "order.placed"
            //      will be delivered to both queues (Product & Notification)
            channel.QueueBind(
                queue: rabbitMqOptions.ProductOrderPlacedQueue,
                exchange: rabbitMqOptions.ExchangeName,
                routingKey: "order.placed");

            channel.QueueBind(
                queue: rabbitMqOptions.NotificationOrderPlacedQueue,
                exchange: rabbitMqOptions.ExchangeName,
                routingKey: "order.placed");
        }
    }
}

