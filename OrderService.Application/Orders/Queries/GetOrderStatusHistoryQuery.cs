using MediatR;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Queries
{
    // CQRS Query:
    // Represents a "READ" operation that retrieves the **status history**
    // (timeline of status changes) for a specific order.

    // When is this used?
    // - In an "Order Details" screen, where you want to show:
    //   * When the order was Created
    //   * When it was Confirmed
    //   * When it was Shipped / Delivered / Cancelled, etc.
    // - Useful for both:
    //   * Customers (tracking their order progress)
    //   * Admins / Support team (audit trail and troubleshooting).

    // What does this query carry?
    // - Only the OrderId, because:
    //   * The handler can use this ID to load all related status-history entries
    //     (from OrderStatusHistory table) from the database.

    // What does it return?
    // - A List<OrderStatusHistoryResponseDTO>
    //   * Each item typically contains:
    //       - OldStatus / NewStatus
    //       - ChangedBy
    //       - ChangedAt (timestamp)
    //       - Remarks / Reason

    // How does MediatR use this?
    // - MediatR will send this query to a handler that implements:
    //     IRequestHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistoryResponseDTO>>
    // - The handler will:
    //     * Fetch status history from the repository using OrderId.
    //     * Map entities to OrderStatusHistoryResponseDTO.
    //     * Return the list.
    public class GetOrderStatusHistoryQuery : IRequest<List<OrderStatusHistoryResponseDTO>>
    {
        // The unique identifier of the order whose status history we want to retrieve.
        public Guid OrderId { get; }

        // Constructor:
        // Creates a new GetOrderStatusHistoryQuery with the given order ID.
        // Parameters:
        // orderId : The ID of the order whose history we want to load.
        public GetOrderStatusHistoryQuery(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}

