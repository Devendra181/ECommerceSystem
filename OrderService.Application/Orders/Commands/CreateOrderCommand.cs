using MediatR;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Commands
{
    // CQRS Command:
    // Represents a "WRITE" operation (i.e., it changes system state).
    // This command encapsulates everything needed to create an order:
    // 1) The order creation request payload (CreateOrderRequestDTO)
    // 2) The AccessToken (so the handler can call other microservices securely if needed)

    // This is a CQRS Command that will be handled by a corresponding Command Handler.
    // - It does NOT contain business logic.
    // - It only carries the data required to perform the "Create Order" operation.
    // - MediatR will pass this object to a handler that implements:
    //     IRequestHandler<CreateOrderCommand, OrderResponseDTO>
    // - The handler will return an OrderResponseDTO as the result.
    public class CreateOrderCommand : IRequest<OrderResponseDTO>
    {
        // The payload sent from the API/client containing all
        // the necessary information to create an order (user, items, addresses, etc.).
        // This is read-only (only getter) so the command is immutable after creation,
        // which is a good CQRS practice (commands should not change after being created).
        public CreateOrderRequestDTO Request { get; }

        // JWT / access token of the current user.
        // - The command handler might need to call other microservices (UserService, ProductService, PaymentService)
        //   using the user's identity/authorization context.
        // - This keeps the handler self-sufficient without depending on HttpContext.
        public string AccessToken { get; }

        // Initializes a new instance of the CreateOrderCommand class.
        // Parameters:
        // request: The input DTO coming from the client, containing order details.
        // accessToken: The bearer token used for downstream service calls.
        public CreateOrderCommand(CreateOrderRequestDTO request, string accessToken)
        {
            // Ensure the request DTO is not null.
            // If it is null, the command is invalid because we cannot create an order without data.
            Request = request ?? throw new ArgumentNullException(nameof(request));

            // Ensure access token is not null.
            // If token is null, handler may fail when calling other services or validating the user.
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }
    }
}
