using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Microsoft.Extensions.Options;
using OrchestratorService.Contracts.Messaging;
namespace OrchestratorService.Infrastructure.Messaging.Producers
{
    // Purpose:
    //   Implements the IOrderEventsPublisher contract.
    //   This class acts as the Orchestrator’s communication channel
    //   to publish Saga-related events into RabbitMQ.
    //
    //   The OrchestratorService uses this publisher to trigger the
    //   next steps in the distributed transaction flow:
    //     1️. OrderPlaced → StockReservationRequested
    //     2️. StockReserved → OrderConfirmed
    //     3️. StockReservationFailed → OrderCancelled
    public sealed class OrderEventsPublisher : IOrderEventsPublisher
    {
        // --------------------------------------------------------
        // Dependencies:
        // --------------------------------------------------------

        // IPublisher is a shared abstraction responsible for publishing the event
        private readonly IPublisher _publisher;

        // Holds RabbitMQ configuration values from appsettings.json,
        // including exchange name and routing keys.
        private readonly RabbitMqOptions _options;

        // --------------------------------------------------------
        // Constructor
        // Parameters:
        //   - publisher: shared messaging abstraction.
        //   - options: typed RabbitMQ configuration.
        // --------------------------------------------------------
        public OrderEventsPublisher(IPublisher publisher, IOptions<RabbitMqOptions> options)
        {
            _publisher = publisher;
            _options = options.Value; // Extracts actual RabbitMQ config
        }

        // --------------------------------------------------------
        // Method: PublishStockReservationRequestedAsync
        // Description:
        //   Publishes a StockReservationRequestedEvent to ProductService.
        //   This is the *next step* after Orchestrator receives an
        //   OrderPlacedEvent from OrderService.
        //
        // Purpose:
        //   Requests ProductService to check inventory and reserve stock
        //   for the given order items.
        //
        // RabbitMQ Routing:
        //   - Exchange: ecommerce.topic
        //   - Routing Key: stock.reservation.requested
        //
        // Downstream Consumer:
        //   - ProductService → consumes event → checks stock availability.
        //
        // Outcome:
        //   Starts the stock validation phase of the Saga.
        // --------------------------------------------------------
        public Task PublishStockReservationRequestedAsync(StockReservationRequestedEvent evt)
        {
            // Optional developer log for visibility (during local testing).
            Console.WriteLine($"[Publish] → StockReservationRequestedEvent for OrderId={evt.OrderId}");

            // Publish message to the configured exchange & routing key.
            return _publisher.PublishAsync(
                _options.ExchangeName,
                _options.RkStockReservationRequested, // e.g. "stock.reservation.requested"
                evt
            );
        }

        // --------------------------------------------------------
        // Method: PublishOrderConfirmedAsync
        // Description:
        //   Publishes an OrderConfirmedEvent when ProductService
        //   successfully reserves all requested stock items.
        //
        // Purpose:
        //   Informs downstream services that the order passed all
        //   validations and can now be marked as "Confirmed."
        //
        // RabbitMQ Routing:
        //   - Exchange: ecommerce.topic
        //   - Routing Key: order.confirmed
        //
        // Downstream Consumers:
        //   - OrderService → updates order status in DB to Confirmed.
        //   - NotificationService → sends success email/SMS to customer.
        //
        // Outcome:
        //   Completes the Saga successfully (happy path).
        // --------------------------------------------------------
        public Task PublishOrderConfirmedAsync(OrderConfirmedEvent evt)
        {
            Console.WriteLine($"[Publish] → OrderConfirmedEvent for OrderId={evt.OrderId}");

            return _publisher.PublishAsync(
                _options.ExchangeName,
                _options.RkOrderConfirmed, // e.g. "order.confirmed"
                evt
            );
        }

        // --------------------------------------------------------
        // Method: PublishOrderCancelledAsync
        // Description:
        //   Publishes an OrderCancelledEvent when ProductService fails
        //   to reserve stock or an orchestration rule triggers compensation.
        //
        // Purpose:
        //   Notifies downstream services that the order cannot be completed
        //   and must be marked as Cancelled.
        //
        // RabbitMQ Routing:
        //   - Exchange: ecommerce.topic
        //   - Routing Key: order.cancelled
        //
        // Downstream Consumers:
        //   - OrderService → compensates (updates DB status to Cancelled).
        //   - NotificationService → alerts the customer with the reason.
        //
        // Outcome:
        //   Ends the Saga in compensation mode (failure path).
        // --------------------------------------------------------
        public Task PublishOrderCancelledAsync(OrderCancelledEvent evt)
        {
            Console.WriteLine($"[Publish] → OrderCancelledEvent for OrderId={evt.OrderId}");

            return _publisher.PublishAsync(
                _options.ExchangeName,
                _options.RkOrderCancelled, // e.g. "order.cancelled"
                evt
            );
        }
    }
}