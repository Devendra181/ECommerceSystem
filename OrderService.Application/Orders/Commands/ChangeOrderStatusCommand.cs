using MediatR;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Commands
{
    // CQRS Command:
    // Represents a "WRITE" operation that changes the status of an existing order.
    //
    // When is this used?
    // - When an admin or a system process wants to:
    //   * Move an order from Pending -> Shipped
    //   * Shipped -> Delivered
    //   * Pending/Confirmed -> Cancelled, etc.
    //
    // What does this command carry?
    // - A ChangeOrderStatusRequestDTO that contains:
    //   * OrderId       : Which order to update
    //   * NewStatus     : The new status (enum)
    //   * ChangedBy     : Who is performing the change (user/system)
    //   * Remarks       : Optional comments or reason
    //
    // IMPORTANT:
    // - This command does NOT contain business logic itself.
    // - It only represents the "intent" to change the status.
    // - MediatR will forward this to a handler that implements:
    //     IRequestHandler<ChangeOrderStatusCommand, ChangeOrderStatusResponseDTO>
    // - The handler will:
    //   * Validate the order
    //   * Apply the new status
    //   * Persist changes
    //   * And return a ChangeOrderStatusResponseDTO describing the outcome.
    public class ChangeOrderStatusCommand : IRequest<ChangeOrderStatusResponseDTO>
    {
        // The payload sent from the API/client containing the details required
        // to change the order status.
        // Typical fields inside ChangeOrderStatusRequestDTO might be:
        // - OrderId
        // - NewStatus
        // - Reason / Remarks
        // - UpdatedBy (optional)

        // The property is read-only (getter only), so once the command is created,
        // you cannot change the request inside it. This is a good CQRS practice:
        // commands should be immutable after creation.
        public ChangeOrderStatusRequestDTO Request { get; }

        // Constructor:
        // Creates a new ChangeOrderStatusCommand with the provided request DTO.
        // Parameters:
        // request : The DTO that contains all details required to change the order status.
        public ChangeOrderStatusCommand(ChangeOrderStatusRequestDTO request)
        {
            // Validate that the request is not null.
            // If the request is null, the command is invalid because we don't know:
            // - which order to update
            // - what status to set
            // - who is performing the change, etc.
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
