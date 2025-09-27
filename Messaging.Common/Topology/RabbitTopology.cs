using Messaging.Common.Options;
using RabbitMQ.Client;

namespace Messaging.Common.Topology
{
    public static class RabbitTopology
    {
        public static void EnsureAll(IModel ch, RabbitMqOptions opt)
        {
            // Main topic exchange for business events
            //    - Durable: survives broker restarts
            //    - AutoDelete: false means it won't disappear when unused
            //    - Type: Topic exchange routes messages based on pattern matching
            ch.ExchangeDeclare(
                exchange: opt.ExchangeName, 
                type: ExchangeType.Topic, 
                durable: true,
                autoDelete: false);

            // Dead Letter exchange + queue
            //  Declare the Dead Letter Exchange (DLX) if configured
            //    - Used for failed/rejected messages (safety net)
            if (!string.IsNullOrWhiteSpace(opt.DlxExchangeName))
            {
                ch.ExchangeDeclare(
                    exchange: opt.DlxExchangeName, 
                    type: ExchangeType.Fanout,   //Fanout: send dead letters to all bound queues
                    durable: true, 
                    autoDelete: false);

                // Declare Dead Letter Queue if provided
                if (!string.IsNullOrWhiteSpace(opt.DlxQueueName))
                {
                    ch.QueueDeclare(
                        queue: opt.DlxQueueName!,
                        durable: true,      // durable: survive broker restarts
                        exclusive: false,   // exclusive: can be consumed by multiple consumers
                        autoDelete: false,  // autoDelete: not auto-deleted when last consumer disconnects
                        arguments: null
                        );  

                    // Bind DLQ to DLX (routingKey irrelevant for fanout exchange)
                    ch.QueueBind(queue: opt.DlxQueueName, exchange: opt.DlxExchangeName, routingKey: "");
                }
            }
            // Common args to attach DLX to business queues
            // Common queue arguments (applied to business queues)
            //    - Add DLX binding if one exists, so rejected messages are routed safely
            //var qargs = new Dictionary<string, object> { ["x-dead-letter-exchange"] = opt.DlxExchangeName };
            var qargs = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(opt.DlxExchangeName))
                qargs["x-dead-letter-exchange"] = opt.DlxExchangeName;
            //args["x-message-ttl"] = 10000;
            //args["x-max-length"] = 100;

            // Declare queues (per consumer group) and bind to routing keys

            // Orchestrator listens to "order.placed"
            ch.QueueDeclare(opt.QOrchestratorOrderPlaced, true, false, false, qargs);
            ch.QueueBind(opt.QOrchestratorOrderPlaced, opt.ExchangeName, opt.RkOrderPlaced);

            // Product listens to orchestrator's reservation request
            ch.QueueDeclare(opt.QProductStockReservationRequested, true, false, false, qargs);
            ch.QueueBind(opt.QProductStockReservationRequested, opt.ExchangeName, opt.RkStockReservationRequested);

            // Orchestrator listens to ProductService outcomes
            ch.QueueDeclare(opt.QOrchestratorStockReserved, true, false, false, qargs);
            ch.QueueBind(opt.QOrchestratorStockReserved, opt.ExchangeName, opt.RkStockReserved);

            ch.QueueDeclare(opt.QOrchestratorStockFailed, true, false, false, qargs);
            ch.QueueBind(opt.QOrchestratorStockFailed, opt.ExchangeName, opt.RkStockFailed);

            // Notification listens to final outcomes
            ch.QueueDeclare(opt.QNotificationOrderConfirmed, true, false, false, qargs);
            ch.QueueBind(opt.QNotificationOrderConfirmed, opt.ExchangeName, opt.RkOrderConfirmed);

            ch.QueueDeclare(opt.QNotificationOrderCancelled, true, false, false, qargs);
            ch.QueueBind(opt.QNotificationOrderCancelled, opt.ExchangeName, opt.RkOrderCancelled);

            // OrderService listens to cancellation for compensation
            ch.QueueDeclare(opt.QOrderCompensationCancelled, true, false, false, qargs);
            ch.QueueBind(opt.QOrderCompensationCancelled, opt.ExchangeName, opt.RkOrderCancelled);
        }

    }
}

