using MediatR;
using Messaging.Common.Events;
using Messaging.Common.Models;
using Microsoft.Extensions.Logging;
using OrderService.Application.Orders.Commands;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Contracts.ExternalServices;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Commands
{
    // CQRS Command Handler:
    // Handles the ConfirmOrderCommand and returns a boolean indicating success.

    // Business meaning:
    // - This is the "WRITE" side operation that:
    //   1) Validates that an order is in a Pending state.
    //   2) Verifies payment status with the Payment Microservice.
    //   3) Updates the order status to Confirmed in the database.
    //   4) Publishes an OrderPlacedEvent to RabbitMQ so:
    //        - ProductService can reserve/reduce stock.
    //        - NotificationService can send email/SMS/notification to the user.

    // Why a handler (and not just a service method)?
    // - To implement CQRS with MediatR:
    //   * Controller/Service sends ConfirmOrderCommand.
    //   * MediatR routes it to this handler.
    //   * This handler performs the full confirmation process.
    public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, bool>
    {
        // Repository for accessing and updating Order data in the database.
        private readonly IOrderRepository _orderRepository;

        // Client for calling the Payment Microservice.
        // - Used to verify that the payment for this order is completed.
        private readonly IPaymentServiceClient _paymentServiceClient;

        // Client for calling the User Microservice.
        // - Used to fetch user details (name, email, phone) for event/notification.
        private readonly IUserServiceClient _userServiceClient;

        // Abstraction over RabbitMQ publisher for OrderPlacedEvent.
        // - Sends the integration event to the broker so other services can react.
        private readonly IOrderPlacedEventPublisher _publisher;

        // Logger for capturing errors and useful diagnostic information.
        private readonly ILogger<ConfirmOrderCommandHandler> _logger;

        // Constructor:
        // ------------
        // All dependencies are injected via DI.
        //
        // Parameters:
        // orderRepository       : For reading/updating orders.
        // paymentServiceClient  : For fetching payment status.
        // userServiceClient     : For fetching user details.
        // publisher             : For publishing OrderPlacedEvent to RabbitMQ.
        // logger                : For logging errors and debugging info.
        public ConfirmOrderCommandHandler(
            IOrderRepository orderRepository,
            IPaymentServiceClient paymentServiceClient,
            IUserServiceClient userServiceClient,
            IOrderPlacedEventPublisher publisher,
            ILogger<ConfirmOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
            _userServiceClient = userServiceClient ?? throw new ArgumentNullException(nameof(userServiceClient));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Handle method:
        // --------------
        // This is the core logic executed when ConfirmOrderCommand is sent.
        //
        // Steps:
        // 1) Load order by ID.
        // 2) Ensure order is in Pending status.
        // 3) Verify payment info from PaymentService.
        // 4) Ensure payment is Completed.
        // 5) Fetch user details from UserService.
        // 6) Change order status to Confirmed in database.
        // 7) Publish OrderPlacedEvent to RabbitMQ.
        //
        // Returns:
        // - true  : if everything succeeds (including status update + event publishing).
        // - throws: if any validation or operation fails (exceptions are logged).
        public async Task<bool> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
        {
            // Extract data from the command.
            var orderId = command.OrderId;
            var accessToken = command.AccessToken;

            // 1. Retrieve the order by its ID from the repository (database).
            //    If no order is found, something is wrong: either a bad ID or stale request.
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // 2. Only allow confirmation if the order is still in Pending state.
            //    - This prevents:
            //      * Double confirmation.
            //      * Confirming orders that are cancelled or already shipped, etc.
            if (order.OrderStatusId != (int)Domain.Enums.OrderStatusEnum.Pending)
                throw new InvalidOperationException("Order is not in a pending state.");

            // 3. Call the Payment Microservice to fetch the latest payment info for this order.
            //    - We pass OrderId + AccessToken to ensure:
            //      * Correct payment record is loaded.
            //      * Authorization is respected by PaymentService.
            var paymentInfo = await _paymentServiceClient.GetPaymentInfoAsync(
                new PaymentInfoRequestDTO { OrderId = orderId }, accessToken);

            // If PaymentService returns null, we cannot safely confirm the order.
            if (paymentInfo == null)
                throw new InvalidOperationException("Payment information not found for this order.");

            // 4. Ensure payment was successfully completed before confirming the order.
            //    - Business rule: Online orders must only be confirmed if PaymentStatus == Completed.
            //    - If payment is Pending/Failed/Cancelled, we refuse to confirm.
            if (paymentInfo.PaymentStatus != PaymentStatusEnum.Completed)
                throw new InvalidOperationException("Payment is not successful.");

            // 5. Get the user details via User Microservice.
            //    - Needed for:
            //      * Filling event fields (CustomerName, Email, Phone).
            //      * NotificationService to send user-specific messages.
            var user = await _userServiceClient.GetUserByIdAsync(order.UserId, accessToken);
            if (user == null)
                throw new InvalidOperationException("User does not exist.");

            try
            {
                // 6. Change the order status in the database to "Confirmed".
                //    - Source: "PaymentService" (because confirmation is driven by payment success).
                //    - Remarks: For audit trail / order status history.
                bool statusChanged = await _orderRepository.ChangeOrderStatusAsync(
                    orderId,
                    Domain.Enums.OrderStatusEnum.Confirmed,
                    "PaymentService",
                    "Payment successful, order confirmed.");

                // If DB update failed, treat it as a hard failure.
                if (!statusChanged)
                    throw new InvalidOperationException("Failed to update order status.");

                // 7. Create the integration event payload that downstream services need.
                //    - ProductService will:
                //        * Reserve/reduce stock for each item.
                //    - NotificationService will:
                //        * Send confirmation email/SMS/app notification.
                var orderPlacedEvent = new OrderPlacedEvent
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    CustomerName = user.FullName,
                    CustomerEmail = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    TotalAmount = order.TotalAmount,
                    CorrelationId = order.Id.ToString(),
                    Items = order.OrderItems.Select(i => new OrderLineItem
                    {
                        ProductId = i.ProductId,
                       // Name = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.PriceAtPurchase
                    }).ToList()
                };

                // 8. Publish the OrderPlacedEvent to RabbitMQ using the shared publisher.
                //    - Under the hood:
                //        * It sends the message to an exchange (e.g., "ecommerce.topic").
                //        * With a routing key (e.g., "order.placed").
                //    - Multiple services can subscribe to this event.
                // - ProductService will consume this event to reduce stock
                // - NotificationService will consume this event to insert a notification
                await _publisher.PublishOrderPlacedAsync(orderPlacedEvent);

                // Everything succeeded: status updated + event published.
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception with context (OrderId) to help with debugging.
                _logger.LogError(ex, "Error while confirming order {OrderId}", orderId);

                // Re-throw so upper layers (API, orchestrator, etc.) can handle it properly:
                // - Return appropriate HTTP status code.
                // - Trigger compensating actions if needed.
                throw;
            }
        }
    }
}
