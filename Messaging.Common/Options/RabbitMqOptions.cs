namespace Messaging.Common.Options
{
    public sealed class RabbitMqOptions
    {
        // Connection settings (per environment)
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "ecommerce_user";
        public string Password { get; set; } = "Test@1234";
        public string VirtualHost { get; set; } = "ecommerce_vhost";

        // Exchanges (topic) & Dead-lettering
        public string ExchangeName { get; set; } = "ecommerce.topic";


        // Dead-lettering (optional but recommended)
        public string? DlxExchangeName { get; set; } = "ecommerce.dlx";
        public string? DlxQueueName { get; set; } = "ecommerce.dlq";


        // Routing keys (keep shared & stable)
        public string RkOrderPlaced { get; set; } = "order.placed";
        public string RkStockReservationRequested { get; set; } = "stock.reservation.requested";
        public string RkStockReserved { get; set; } = "stock.reserved";
        public string RkStockFailed { get; set; } = "stock.reservation_failed";
        public string RkOrderConfirmed { get; set; } = "order.confirmed";
        public string RkOrderCancelled { get; set; } = "order.cancelled";

        // Queues (one per consumer group)
        public string QOrchestratorOrderPlaced { get; set; } = "orchestrator.order_placed";
        public string QProductStockReservationRequested { get; set; } = "product.stock_reservation_requested";
        public string QOrchestratorStockReserved { get; set; } = "orchestrator.stock_reserved";
        public string QOrchestratorStockFailed { get; set; } = "orchestrator.stock_failed";
        public string QNotificationOrderConfirmed { get; set; } = "notification.order_confirmed";
        public string QNotificationOrderCancelled { get; set; } = "notification.order_cancelled";
        public string QOrderCompensationCancelled { get; set; } = "order.compensation_cancelled";
    }
}
