namespace Messaging.Common.Options
{
    // RabbitMqOptions defines all configurable settings used by RabbitMQ
    // across the microservices in the e-commerce system.

    // Every service reads these values (usually from appsettings.json)
    // to ensure consistent naming of exchanges, queues, and routing keys.

    // This ensures all services use consistent naming and connection parameters.
    // Think of this as the "single source of truth" for RabbitMQ configuration.
    public sealed class RabbitMqOptions
    {
        // --------------------------------------------------------------------
        // Connection Settings
        // --------------------------------------------------------------------
        // These properties define how the application connects to the RabbitMQ broker.
        // They remain consistent across all microservices so each service
        // can connect to the same RabbitMQ instance securely and reliably.

        // The hostname or IP address of the RabbitMQ broker.
        // Example: "localhost" for local dev or a server name in production.
        public string HostName { get; set; } = "localhost";

        // The default port for AMQP (5672).
        // You can change it if RabbitMQ runs on a different port.
        public int Port { get; set; } = 5672;

        // The virtual host (vhost) in RabbitMQ.
        // Virtual host name used for logical isolation within RabbitMQ.
        // Think of a vhost as a "namespace" for your queues and exchanges.
        public string VirtualHost { get; set; } = "ecommerce_vhost";

        // The username used to authenticate with the RabbitMQ broker.
        // Ensure this account has permissions for the specified virtual host.
        public string UserName { get; set; } = "ecommerce_user";

        // The password for the RabbitMQ user account.
        // Should be stored securely (in environment variables or secret store).
        public string Password { get; set; } = "Test@1234";

        // --------------------------------------------------------------------
        // Exchanges & Dead-Letter Configuration
        // --------------------------------------------------------------------
        // Exchanges are the entry points for messages in RabbitMQ.
        // The main exchange routes business events (like order placed, stock reserved, etc.),
        // while the DLX (Dead Letter Exchange) captures failed or rejected messages.

        // The main topic exchange where all domain events are published (order placed, stock reserved, etc.).
        public string ExchangeName { get; set; } = "ecommerce.topic";

        // The Dead Letter Exchange (DLX) that receives messages which
        // cannot be processed or are explicitly rejected by consumers.
        public string? DlxExchangeName { get; set; } = "ecommerce.dlx";

        // The Dead Letter Queue (DLQ) bound to the DLX.
        // All failed or unprocessed messages are stored here for later inspection.
        public string? DlxQueueName { get; set; } = "ecommerce.dlq";

        // --------------------------------------------------------------------
        // Routing Keys
        // --------------------------------------------------------------------
        // Routing keys define the "address" or "topic" of each message.
        // They help RabbitMQ determine which queues should receive a message.
        // Each key corresponds to a specific event in the Saga flow.

        // Routing key used when an order is placed by the OrderService.
        // Fired by OrderService → Consumed by Orchestrator
        public string RkOrderPlaced { get; set; } = "order.placed";

        // Routing key used when the Orchestrator requests stock reservation from the ProductService.
        // Fired by Orchestrator → Consumed by ProductService
        public string RkStockReservationRequested { get; set; } = "stock.reservation.requested";

        // Routing key used when ProductService confirms that stock was successfully reserved.
        // Fired by ProductService → Consumed by Orchestrator (success path)
        public string RkStockReserved { get; set; } = "stock.reserved";

        // Routing key used when ProductService fails to reserve stock (e.g., insufficient quantity).
        // Fired by ProductService → Consumed by Orchestrator (failure path)
        public string RkStockFailed { get; set; } = "stock.reservation_failed";

        // Routing key used when Orchestrator confirms an order after successful stock reservation.
        // Fired by Orchestrator → Consumed by OrderService & NotificationService (success)
        public string RkOrderConfirmed { get; set; } = "order.confirmed";

        // Routing key used when Orchestrator cancels an order due to stock failure.
        // Fired by Orchestrator → Consumed by OrderService & NotificationService (failure)
        public string RkOrderCancelled { get; set; } = "order.cancelled";

        // --------------------------------------------------------------------
        // Queue Names
        // --------------------------------------------------------------------
        // Queues are where consumers (services) actually listen for messages.
        // Each microservice has its own dedicated queues based on the events it handles.
        // Queues are bound to routing keys to receive relevant messages.
        // Naming convention: [service].[event_purpose]

        // Queue where the OrchestratorService listens for "order.placed" events.
        // This is the entry point for the Saga process.
        public string QOrchestratorOrderPlaced { get; set; } = "orchestrator.order_placed";

        // Queue where the ProductService listens for stock reservation requests.
        public string QProductStockReservationRequested { get; set; } = "product.stock_reservation_requested";

        // Queue where the OrchestratorService listens for "stock.reserved" events
        // (successful reservation confirmation).
        public string QOrchestratorStockReserved { get; set; } = "orchestrator.stock_reserved";

        // Queue where the OrchestratorService listens for "stock.reservation_failed" events
        // (failed stock reservation).
        public string QOrchestratorStockFailed { get; set; } = "orchestrator.stock_failed";

        // Queue where the NotificationService listens for "order.confirmed" events
        // to send confirmation messages to customers.
        public string QNotificationOrderConfirmed { get; set; } = "notification.order_confirmed";

        // Queue where the NotificationService listens for "order.cancelled" events
        // to send cancellation messages to customers.
        public string QNotificationOrderCancelled { get; set; } = "notification.order_cancelled";

        // Queue where the OrderService listens for "order.cancelled" events
        // to perform compensation logic (rollback or status update).
        public string QOrderCompensationCancelled { get; set; } = "order.compensation_cancelled";
    }
}
