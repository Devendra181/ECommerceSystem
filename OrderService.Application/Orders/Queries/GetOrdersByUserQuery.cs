using MediatR;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Queries
{
    // CQRS Query:
    // Represents a "READ" operation to fetch a list of orders for a specific user,
    // with pagination support.

    // What does this query carry?
    // - UserId     : Whose orders we want.
    // - PageNumber : Which page of results we want.
    // - PageSize   : How many records per page.

    // What is returned?
    // - PaginatedResultDTO<OrderResponseDTO>
    //   * Items      : List of OrderResponseDTO for the requested page.
    //   * PageNumber : Current page index.
    //   * PageSize   : Size of each page.
    //   * TotalCount : Total number of orders for that user (for pagination UI).

    // How does MediatR use this?
    // - MediatR will send this query to a handler that implements:
    //     IRequestHandler<GetOrdersByUserQuery, PaginatedResultDTO<OrderResponseDTO>>
    // - The handler will:
    //     * Call the repository method (e.g., GetByUserIdAsync).
    //     * Apply pagination.
    //     * Map entities to OrderResponseDTO.
    //     * Wrap everything in a PaginatedResultDTO and return it.
    public class GetOrdersByUserQuery : IRequest<PaginatedResultDTO<OrderResponseDTO>>
    {
        // The unique identifier of the user whose orders we want to retrieve.
        // This is typically the "Owner" of the orders:
        public Guid UserId { get; }

        // PageNumber determines which "page" of data to fetch.
        // Example: PageNumber = 1 means the first page.
        public int PageNumber { get; }

        // PageSize determines how many orders to return per page.
        // Example: PageSize = 20 means return 20 orders per page.
        public int PageSize { get; }

        // Constructor:
        // Creates a new GetOrdersByUserQuery with user ID and pagination info.
        // Parameters:
        // userId     : The user whose orders we want to fetch.
        // pageNumber : Which page of results to fetch (defaults to 1).
        // pageSize   : How many records per page (defaults to 20).
        public GetOrdersByUserQuery(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            // Assign the user id.
            UserId = userId;

            // Defensive defaults:
            // - If pageNumber is 0 or negative, fallback to 1.
            //   This prevents invalid pagination inputs from causing issues in the handler or repository.
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;

            // - If pageSize is 0 or negative, fallback to 20.
            //   This ensures we always have a reasonable page size.
            PageSize = pageSize <= 0 ? 20 : pageSize;
        }
    }
}
