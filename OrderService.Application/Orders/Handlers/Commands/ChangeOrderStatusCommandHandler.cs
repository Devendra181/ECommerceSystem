using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Commands;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Commands
{
    // CQRS Command Handler:
    // Handles the ChangeOrderStatusCommand and returns a ChangeOrderStatusResponseDTO.

    // Business meaning:
    // - This is the "WRITE" side operation that changes the status of an existing order.
    // - Typical use cases:
    //   * Admin marks an order as Shipped / Delivered / Cancelled.
    //   * System processes (e.g., scheduled jobs) update stale orders.

    // Key responsibilities:
    // - Validate that the order exists.
    // - Validate that the new status is different from the current status.
    // - Persist the new status and status history.
    // - Return a detailed result object describing what happened.
    public class ChangeOrderStatusCommandHandler :
        IRequestHandler<ChangeOrderStatusCommand, ChangeOrderStatusResponseDTO>
    {
        // Repository abstraction for accessing and updating orders in the database.
        private readonly IOrderRepository _orderRepository;

        // Logger to record errors and operational information.
        private readonly ILogger<ChangeOrderStatusCommandHandler> _logger;

        // Constructor:
        // Dependencies are injected via DI.
        // Parameters:
        // orderRepository : Used to load and update orders.
        // logger          : Used to log exceptions and important events.
        public ChangeOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            ILogger<ChangeOrderStatusCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Handle method:
        // Executes when a ChangeOrderStatusCommand is sent through MediatR.
        // Steps:
        // 1) Build an initial response object based on the request.
        // 2) Fetch the order and validate its existence.
        // 3) Check current status vs. requested new status.
        // 4) If valid, update the status using the repository.
        // 5) Fill the response (Success/Error) and return it.

        // Returns:
        // - ChangeOrderStatusResponseDTO:
        //   * Contains OldStatus, NewStatus, ChangedBy, Remarks, ChangedAt, Success flag, ErrorMessage.
        public async Task<ChangeOrderStatusResponseDTO> Handle(
            ChangeOrderStatusCommand command,
            CancellationToken cancellationToken)
        {
            // 1. Extract the request DTO from the command.
            //    - The request has: OrderId, NewStatus, ChangedBy, Remarks, etc.
            var request = command.Request;

            // 2. Initialize the response with the basic info from the request.
            //    - We set Success = false by default and only flip it to true if everything goes well.
            var response = new ChangeOrderStatusResponseDTO
            {
                OrderId = request.OrderId,
                NewStatus = request.NewStatus,
                ChangedBy = request.ChangedBy,
                Remarks = request.Remarks,
                ChangedAt = DateTime.UtcNow,
                Success = false
            };

            try
            {
                // 3. Fetch the current order from the database.
                //    - We need the current status and to confirm the order actually exists.
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    // Order not found – we return a failure response with an appropriate message.
                    response.ErrorMessage = "Order not found.";
                    return response;
                }

                // 4. Capture the current status (old status) of the order.
                //    - This is useful for:
                //       * Returning in the response.
                //       * Logging/auditing.
                var oldStatus = (OrderStatusEnum)order.OrderStatusId;
                response.OldStatus = oldStatus;

                // 5. Prevent no-op updates:
                //    - If the order is already in the requested status, there's nothing to change.
                //    - We treat this as a failure (or a validation warning), not a success.
                if (oldStatus == request.NewStatus)
                {
                    response.ErrorMessage = "Order is already in the requested status.";
                    return response;
                }

                // 6. Attempt to change the status in the repository.
                //    - The repository should:
                //       * Update the Order table.
                //       * Optionally insert into an OrderStatusHistory table.
                bool statusChanged = await _orderRepository.ChangeOrderStatusAsync(
                    request.OrderId,
                    request.NewStatus,
                    request.ChangedBy ?? "System", // Fallback to "System" if ChangedBy is null.
                    request.Remarks);

                // If the repository reports failure, we return an error response.
                if (!statusChanged)
                {
                    response.ErrorMessage = "Failed to update order status.";
                    return response;
                }

                // 7. At this point, the status update succeeded.
                //    - Here is where you could trigger:
                //       * Notifications (email/SMS).
                //       * Domain events / integration events.
                //    - Mark the operation as successful.
                // TODO: Trigger notifications or other side-effects based on new status
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                // 8. If any unexpected exception occurs:
                //    - Log the error with context (OrderId).
                //    - Fill the response with an error message.
                //    - Return the response with Success = false.
                _logger.LogError(ex, "Error while changing order status for {OrderId}", request.OrderId);
                response.ErrorMessage = $"Exception: {ex.Message}";
                return response;
            }
        }
    }
}
