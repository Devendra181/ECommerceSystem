using Messaging.Common.Options;
using RabbitMQ.Client;

namespace Messaging.Common.Topology
{
    // The RabbitTopology class ensures that all required exchanges, queues, 
    // and bindings exist before any microservice starts publishing or consuming messages.
    // It is idempotent — meaning if the objects already exist, it won’t recreate them.
    public static class RabbitTopology
    {
        // Declares and binds all exchanges and queues required across microservices.
        // This method is typically called once during service startup.

        // Parameters:
        //    ch: An active RabbitMQ channel (IModel) used to declare exchanges and queues.
        //    opt: Configuration options containing exchange, queue, and routing key names
        public static void EnsureAll(IModel ch, RabbitMqOptions opt)
        {
            // -------------------------------------------------------------
            // 1. Declare Main topic exchange for business events
            // --------------------------------------------------------------------
            // This is the central topic exchange where all domain events are published.
            // Services will publish or subscribe to routing keys on this exchange.
            // Using 'durable: true' ensures that the exchange survives RabbitMQ restarts.
            
            //    - Durable: survives broker restarts
            //    - AutoDelete: false means it won't disappear when unused
            //    - Type: Topic exchange routes messages based on pattern matching
            ch.ExchangeDeclare(
                exchange: opt.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // -------------------------------------------------------------
            // 2. Declare Dead Letter Exchange (DLX) and Queue if configured
            // --------------------------------------------------------------------
            // The DLX handles messages that are rejected, expired, or failed to be processed.
            // A fanout exchange broadcasts all dead messages to the DLQ (Dead Letter Queue).
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
                    // Create the DLQ (Dead Letter Queue) to store failed messages for later inspection.
                    ch.QueueDeclare(queue: opt.DlxQueueName, durable: true, exclusive: false, autoDelete: false);
                    ch.QueueDeclare(
                        queue: opt.DlxQueueName!,
                        durable: true,      // durable: survive broker restarts
                        exclusive: false,   // exclusive: Queue is shared. Multiple connections or consumers (even from different apps/machines) can access it. Queue persists even if the declaring connection closes.
                                            // exclusive: true: Queue can only be used by the connection that declared it. When that connection closes → queue is automatically deleted.
                        autoDelete: false,  // autoDelete: not auto-deleted when last consumer disconnects
                        arguments: null
                        );

                    // Bind DLQ to DLX (so that dead messages are redirected here).
                    //routingKey irrelevant for fanout exchange
                    ch.QueueBind(queue: opt.DlxQueueName, exchange: opt.DlxExchangeName, routingKey: "");
                }
            }

            // -------------------------------------------------------------
            // 3. Attach DLX Settings to Business Queues
            // -------------------------------------------------------------
            // These arguments attach the DLX to all main business queues.
            // It ensures that if a consumer rejects a message, RabbitMQ automatically
            // sends it to the Dead Letter Exchange (DLX) for safe storage and inspection.
            //var qargs = new Dictionary<string, object> { ["x-dead-letter-exchange"] = opt.DlxExchangeName };
            var qargs = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = opt.DlxExchangeName!, // Where to send after failure
                ["x-max-length"] = 1000,                           // Max messages
                ["x-message-ttl"] = 300000,                        // 5-minute lifespan
                //RabbitMQ does not support maximum number of retries
            };

            // -------------------------------------------------------------
            // 4️. Declare & Bind Queues for Each Microservice
            // -------------------------------------------------------------
            // ORCHESTRATOR → Listens for "order.placed" events published by OrderService.
            // This is where the Saga begins. Once an order is placed, the orchestrator takes over.
            ch.QueueDeclare(queue: opt.QOrchestratorOrderPlaced, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            // This means:
            //      Durable → queue survives broker restarts.
            //      Exclusive = false → multiple microservices or consumers can listen to the same queue.
            //      AutoDelete = false → queue stays alive until manually deleted or the broker is reset.

            ch.QueueBind(queue: opt.QOrchestratorOrderPlaced, exchange: opt.ExchangeName, routingKey: opt.RkOrderPlaced);

            // PRODUCT SERVICE → Listens for "stock.reservation.requested" events.
            // The orchestrator requests the ProductService to reserve stock for an order.
            ch.QueueDeclare(queue: opt.QProductStockReservationRequested, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QProductStockReservationRequested, exchange: opt.ExchangeName, routingKey: opt.RkStockReservationRequested);

            // ORCHESTRATOR listens to "stock.reserved" events (from ProductService)
            //    On success, Orchestrator will confirm the order.
            ch.QueueDeclare(queue: opt.QOrchestratorStockReserved, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QOrchestratorStockReserved, exchange: opt.ExchangeName, routingKey: opt.RkStockReserved);

            // ORCHESTRATOR also listens to "stock.failed" events (from ProductService)
            //    On failure, Orchestrator will cancel the order and trigger compensation.
            ch.QueueDeclare(queue: opt.QOrchestratorStockFailed, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QOrchestratorStockFailed, exchange: opt.ExchangeName, routingKey: opt.RkStockFailed);

            // -------------------------------------------------------------
            // 5️. Declare Notification Service Queues
            // -------------------------------------------------------------
            // NotificationService listens to "order.confirmed" events
            //    Used to send confirmation emails or SMS notifications to customers.
            ch.QueueDeclare(queue: opt.QNotificationOrderConfirmed, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QNotificationOrderConfirmed, exchange: opt.ExchangeName, routingKey: opt.RkOrderConfirmed);

            // NotificationService also listens to "order.cancelled" events
            //    Used to send cancellation alerts to customers.
            ch.QueueDeclare(queue: opt.QNotificationOrderCancelled, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QNotificationOrderCancelled, exchange: opt.ExchangeName, routingKey: opt.RkOrderCancelled);

            // -------------------------------------------------------------
            // 6️. Declare Compensation Queue for OrderService
            // -------------------------------------------------------------
            // OrderService listens to "order.cancelled" events
            //    This ensures that failed orders are compensated in the database.
            ch.QueueDeclare(queue: opt.QOrderCompensationCancelled, durable: true, exclusive: false, autoDelete: false, arguments: qargs);
            ch.QueueBind(queue: opt.QOrderCompensationCancelled, exchange: opt.ExchangeName, routingKey: opt.RkOrderCancelled);

            // All topology components (Exchanges, Queues, Bindings) are now ensured.
            // This setup guarantees that services can publish or consume messages
            // safely and consistently across the distributed Saga workflow.
        }
    }
}
