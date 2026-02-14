using MediatR;
namespace OrderService.Application.Orders.Commands
{
    // CQRS Command:
    // Represents a "WRITE" operation because confirming an order changes the system state
    // (for example, OrderStatus changes from Pending -> Confirmed and events may be published).

    // What this command carries:
    // 1) OrderId     -> Which order should be confirmed
    // 2) AccessToken -> So the handler can securely call other microservices (Payment/User/etc.)
    //                  without depending on HttpContext inside the Application layer.

    // MediatR Flow:
    // - This command will be sent using: IMediator.Send(new ConfirmOrderCommand(...))
    // - MediatR will route it to a handler that implements:
    //     IRequestHandler<ConfirmOrderCommand, bool>
    // - The handler returns a bool:
    //     true  => order confirmed successfully
    //     false => confirmation failed (or you may throw exceptions for failures)
    public class ConfirmOrderCommand : IRequest<bool>
    {
        // The unique identifier of the order we want to confirm.
        // - The handler will:
        //   * Load the order from the database using this ID.
        //   * Validate that the order exists and is in "Pending" state.
        //   * Then confirm it if payment is successful.
        public Guid OrderId { get; }

        // JWT / access token of the current caller.
        // - Used by the handler to:
        //   * Call PaymentService to verify payment status.
        //   * Call UserService to fetch user details (for events/notifications).
        // - Passing this token into the command makes the handler self-contained,
        //   so it doesn't need direct access to HttpContext or other web-layer details.
        public string AccessToken { get; }

        // Constructor:
        // Creates a new ConfirmOrderCommand instance with the required data.
        // Parameters:
        // orderId     : The ID of the order that needs to be confirmed.
        // accessToken : The bearer token used for downstream service calls
        //               (PaymentService, UserService, etc.).
        public ConfirmOrderCommand(Guid orderId, string accessToken)
        {
            // Simply assign the order ID.
            OrderId = orderId;

            // Ensure access token is not null.
            // If token is null, handler may fail when calling other services or validating security context.
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }
    }
}
