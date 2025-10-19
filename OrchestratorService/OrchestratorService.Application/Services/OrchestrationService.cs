using Messaging.Common.Events;
using Messaging.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using OrchestratorService.Contracts.Messaging;
namespace OrchestratorService.Application.Services
{
    // Purpose:
    //   This is the core orchestrator that drives the Saga workflow
    //   for order processing in a distributed microservices system.
    //
    //   It listens (indirectly, via consumers) to key events like:
    //     - OrderPlacedEvent
    //     - StockReservedCompletedEvent
    //     - StockReservationFailedEvent
    //
    //   and then decides what to do next — whether to:
    //     Confirm the order, or
    //     Cancel the order (compensate)
    //
    // Responsibilities:
    //   1. Maintain temporary state (cached order details).
    //   2. Publish the next appropriate event in the Saga.
    //   3. Clean up after the Saga flow finishes for an order.
    //
    // Note:
    //   - No database is used here — state is transient, stored in memory.
    //   - In real production environments, use a distributed cache (e.g., Redis)
    //     to ensure durability across orchestrator restarts.
    public sealed class OrchestrationService : IOrchestrationService
    {
        // --------------------------------------------------------
        // Dependencies:
        // --------------------------------------------------------

        // Abstraction for publishing events to RabbitMQ.
        private readonly IOrderEventsPublisher _publisher;

        // In-memory cache used to store intermediate order states.
        // Stores OrderPlacedEvent data temporarily between Saga steps.
        private readonly IMemoryCache _cache;

        // Constructor: injects publisher and memory cache.
        public OrchestrationService(IOrderEventsPublisher publisher, IMemoryCache cache)
        {
            _publisher = publisher;
            _cache = cache;
        }

        // --------------------------------------------------------
        // Method: OnOrderPlacedAsync
        // Description:
        //   Triggered when OrderService publishes an OrderPlacedEvent.
        //   This method marks the beginning of the Saga workflow.
        //
        // Responsibilities:
        //   1. Cache the full order details temporarily
        //      (so they can be reused when stock results arrive).
        //   2. Publish a StockReservationRequestedEvent to ProductService
        //      asking it to verify and reserve inventory.
        //
        // Parameters:
        //   evt → OrderPlacedEvent (contains OrderId, UserId, Items, etc.)
        //
        // Next Step in Saga:
        //   ProductService receives the request and publishes either:
        //     - StockReservedCompletedEvent (success) OR
        //     - StockReservationFailedEvent (failure)
        // --------------------------------------------------------
        public Task OnOrderPlacedAsync(OrderPlacedEvent evt)
        {
            // Cache the order for 30 minutes (temporary Saga state).
            // This helps future events (e.g., customer details in confirmation/cancellation).
            _cache.Set(CacheKey(evt.OrderId), evt, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            // Build and publish a stock reservation request event.
            // ProductService will listen for this and try to reserve stock.
            var request = new StockReservationRequestedEvent
            {
                CorrelationId = evt.CorrelationId, // links all Saga events together
                OrderId = evt.OrderId,
                UserId = evt.UserId,
                Items = evt.Items.ToList()         // list of order line items
            };

            // Publish to the message broker (RabbitMQ).
            return _publisher.PublishStockReservationRequestedAsync(request);
        }

        // --------------------------------------------------------
        // Method: OnStockReservedAsync
        // Description:
        //   Triggered when ProductService successfully reserves stock.
        //
        // Responsibilities:
        //   1. Retrieve cached order details from memory.
        //   2. Build and publish an OrderConfirmedEvent.
        //      - OrderService → updates DB status to Confirmed.
        //      - NotificationService → sends confirmation message.
        //   3. Clear cache since this Saga instance is complete.
        //
        // Parameters:
        //   evt → StockReservedCompletedEvent (contains OrderId and reserved item info)
        //
        // Next Step in Saga:
        //   OrderService & NotificationService react to OrderConfirmedEvent.
        // --------------------------------------------------------
        public async Task OnStockReservedAsync(StockReservedCompletedEvent evt)
        {
            // Try to retrieve the cached order details using OrderId as key.
            if (_cache.TryGetValue(CacheKey(evt.OrderId), out OrderPlacedEvent? placed) && placed is not null)
            {
                // Build OrderConfirmedEvent using both cached and incoming event data.
                var confirmed = new OrderConfirmedEvent
                {
                    CorrelationId = evt.CorrelationId,
                    OrderId = placed.OrderId,
                    UserId = placed.UserId,
                    OrderNumber = placed.OrderNumber,
                    CustomerName = placed.CustomerName,
                    CustomerEmail = placed.CustomerEmail,
                    PhoneNumber = placed.PhoneNumber,
                    TotalAmount = placed.TotalAmount,
                    Items = placed.Items.ToList() // reuse order item details
                };

                // Publish success message → Saga completes successfully.
                await _publisher.PublishOrderConfirmedAsync(confirmed);

                // Remove cached order (Saga instance complete).
                _cache.Remove(CacheKey(evt.OrderId));
            }
        }

        // --------------------------------------------------------
        // Method: OnStockReservationFailedAsync
        // Description:
        //   Triggered when ProductService fails to reserve stock.
        //   This represents the "compensation" path in the Saga.
        //
        // Responsibilities:
        //   1. Retrieve cached order details.
        //   2. Build and publish an OrderCancelledEvent (with reason + failed items).
        //      - OrderService → marks order as Cancelled.
        //      - NotificationService → notifies user of failure.
        //   3. Clear cache once compensation is complete.
        //
        // Parameters:
        //   evt → StockReservationFailedEvent (contains failure reason and failed items)
        //
        // Next Step in Saga:
        //   OrderService & NotificationService react to OrderCancelledEvent.
        // --------------------------------------------------------
        public async Task OnStockReservationFailedAsync(StockReservationFailedEvent evt)
        {
            // Try to retrieve cached order info.
            if (_cache.TryGetValue(CacheKey(evt.OrderId), out OrderPlacedEvent? placed) && placed is not null)
            {
                // Build OrderCancelledEvent with detailed failure info.
                var cancelled = new OrderCancelledEvent
                {
                    CorrelationId = evt.CorrelationId,
                    OrderId = placed.OrderId,
                    UserId = placed.UserId,
                    OrderNumber = placed.OrderNumber,
                    CustomerName = placed.CustomerName,
                    CustomerEmail = placed.CustomerEmail,
                    PhoneNumber = placed.PhoneNumber,
                    TotalAmount = placed.TotalAmount,

                    // Map FailedItems (from ProductService) into shared model for cancellation event.
                    Items = evt.FailedItems?.Select(i => new FailedLineItem
                    {
                        ProductId = i.ProductId,
                        Requested = i.Requested,
                        Available = i.Available,
                        Reason = i.Reason
                    }).ToList() ?? new List<FailedLineItem>(),

                    Reason = evt.Reason // e.g., "Insufficient stock" or "Out of sync inventory"
                };

                // Publish compensation event → triggers order cancellation.
                await _publisher.PublishOrderCancelledAsync(cancelled);

                // Remove cached order (Saga instance complete).
                _cache.Remove(CacheKey(evt.OrderId));
            }
        }

        // --------------------------------------------------------
        // Helper: CacheKey
        // Description:
        //   Generates a unique key for each cached order.
        //   Prevents overlap when multiple orders are processed simultaneously.
        // --------------------------------------------------------
        private static string CacheKey(Guid orderId) => $"order-placed:{orderId}";
    }
}
