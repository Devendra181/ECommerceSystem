using MediatR;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Queries
{
    // CQRS Query:
    // Represents a "READ" operation (it does NOT change system state).

    // When is this used?
    // - Whenever we want to fetch the details of a single order
    //   based on its unique identifier (OrderId).

    // What does this query carry?
    // - Only the OrderId, because that's all we need to look up the order.

    // How does MediatR use this?
    // - MediatR will send this query to a handler that implements:
    //     IRequestHandler<GetOrderByIdQuery, OrderResponseDTO?>
    // - The handler will:
    //     * Use OrderId to fetch the order from the repository/DB.
    //     * Map the entity to OrderResponseDTO.
    //     * Return:
    //         - an OrderResponseDTO if found
    //         - null if no order exists with this ID (hence the nullable type OrderResponseDTO?).
    public class GetOrderByIdQuery : IRequest<OrderResponseDTO?>
    {
        // The unique identifier of the order we want to retrieve.
        public Guid OrderId { get; }

        // Constructor:
        // Creates a new GetOrderByIdQuery with the specified order ID.
        // Parameters:
        // orderId : The ID of the order to fetch from the database.
        public GetOrderByIdQuery(Guid orderId)
        {
            // Store the provided order id.
            OrderId = orderId;
        }
    }
}
