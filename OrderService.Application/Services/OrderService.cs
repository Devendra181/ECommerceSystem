using MediatR;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;

namespace OrderService.Application.Services
{
    // Application Service (OrderService)
    // This class is a thin **facade** over MediatR + CQRS.

    // Why do we keep this service even after introducing CQRS?
    // - To keep controllers and other consumers decoupled from MediatR.
    //   * Controllers depend on IOrderService (clean, business-oriented interface).
    //   * Internally, this service uses MediatR to send commands/queries to handlers.
    // - This keeps the public API of the application very expressive:
    //     IOrderService.CreateOrderAsync(...)
    //     IOrderService.GetOrdersByUserAsync(...)
    //   instead of having MediatR calls scattered everywhere.

    // Responsibilities:
    // - DOES NOT contain business logic.
    // - Only:
    //   * Accepts method calls from controllers (or other services).
    //   * Translates them into appropriate CQRS Commands/Queries.
    //   * Sends them via IMediator.
    //   * Returns the result from handlers.
    public class OrderService : IOrderService
    {
        // MediatR's central interface.
        // - Used to send Commands and Queries.
        // - It will internally locate and execute the correct handler.
        private readonly IMediator _mediator;

        // Constructor:
        // Receives IMediator via Dependency Injection.
        // Parameters:
        // mediator : The MediatR instance used to dispatch commands/queries to their handlers.
        public OrderService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Creates a new order.
        // Flow:
        // - Controller calls: IOrderService.CreateOrderAsync(request, accessToken)
        // - This method:
        //   1) Wraps the data in a CreateOrderCommand.
        //   2) Calls _mediator.Send(command).
        //   3) MediatR routes the command to CreateOrderCommandHandler.
        //   4) Handler executes business logic and returns OrderResponseDTO.
        public Task<OrderResponseDTO> CreateOrderAsync(CreateOrderRequestDTO request, string accessToken)
        {
            // Create a command that represents the intent: "Create a new order".
            var command = new CreateOrderCommand(request, accessToken);

            // Send the command to MediatR, which will invoke the corresponding handler.
            return _mediator.Send(command);
        }

        // Confirms an existing order (typically after successful payment).
        // Flow:
        // - Called when we want to change order status from Pending -> Confirmed
        //   based on payment status in the PaymentService.
        // - This method:
        //   1) Creates a ConfirmOrderCommand with orderId + accessToken.
        //   2) Sends it via MediatR.
        //   3) Handler performs:
        //        * payment status check,
        //        * status update,
        //        * event publishing, etc.
        //   4) Returns bool indicating success/failure (or throws exceptions).
        public Task<bool> ConfirmOrderAsync(Guid orderId, string accessToken)
        {
            var command = new ConfirmOrderCommand(orderId, accessToken);
            return _mediator.Send(command);
        }

        // Changes the status of an order.
        // Flow:
        // - Wraps the ChangeOrderStatusRequestDTO into ChangeOrderStatusCommand.
        // - Sends it via MediatR to ChangeOrderStatusCommandHandler.
        // - Handler:
        //   * Validates the state transition.
        //   * Persists the new status.
        //   * Returns a detailed ChangeOrderStatusResponseDTO.
        public Task<ChangeOrderStatusResponseDTO> ChangeOrderStatusAsync(ChangeOrderStatusRequestDTO request)
        {
            var command = new ChangeOrderStatusCommand(request);
            return _mediator.Send(command);
        }

        // Retrieves a single order by its ID.
        // Flow:
        // - Controller calls this to get details for a specific order.
        // - This method:
        //   1) Creates a GetOrderByIdQuery.
        //   2) Sends it via MediatR.
        //   3) Handler queries the repository and maps the result to OrderResponseDTO.
        //   4) Returns OrderResponseDTO? (nullable if the order does not exist).
        public Task<OrderResponseDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var query = new GetOrderByIdQuery(orderId);
            return _mediator.Send(query);
        }

        // Retrieves a paginated list of orders for a specific user.
        // Flow:
        // - Builds a GetOrdersByUserQuery (userId + pagination).
        // - Sends it via MediatR.
        // - Handler:
        //   * Fetches orders from the repository.
        //   * Maps them to OrderResponseDTO.
        //   * Wraps in PaginatedResultDTO<OrderResponseDTO>.
        public Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersByUserAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = new GetOrdersByUserQuery(userId, pageNumber, pageSize);
            return _mediator.Send(query);
        }

        // Retrieves a filtered, paginated list of orders.
        // Flow:
        // - Wraps the filter in a GetOrdersQuery.
        // - Sends via MediatR.
        // - Handler calls repository GetOrdersWithFiltersAsync and returns
        //   PaginatedResultDTO<OrderResponseDTO>.
        public Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersAsync(OrderFilterRequestDTO filter)
        {
            var query = new GetOrdersQuery(filter);
            return _mediator.Send(query);
        }

        // Retrieves the full status change history (audit trail) of a specific order.
        // Flow:
        // - Wraps the orderId in a GetOrderStatusHistoryQuery.
        // - Sends it through MediatR.
        // - Handler loads history from the repository and maps to a list of
        //   OrderStatusHistoryResponseDTO.
        public Task<List<OrderStatusHistoryResponseDTO>> GetOrderStatusHistoryAsync(Guid orderId)
        {
            var query = new GetOrderStatusHistoryQuery(orderId);
            return _mediator.Send(query);
        }
    }
}
