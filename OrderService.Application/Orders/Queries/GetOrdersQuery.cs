using MediatR;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;

namespace OrderService.Application.Orders.Queries
{
    // CQRS Query:
    // Represents a "READ" operation to fetch a paginated list of orders
    // using flexible filter criteria.

    // When is this used?
    // - In an admin "Order Management" screen:
    //   * Filter by status (Pending, Confirmed, Shipped, Cancelled, etc.)
    //   * Filter by date range (FromDate, ToDate)
    //   * Search by order number or keyword
    //   * Apply pagination (PageNumber, PageSize)

    // What does this query carry?
    // - A single object: OrderFilterRequestDTO (Filter)
    //   * Status       : Optional status filter
    //   * FromDate     : Optional start date
    //   * ToDate       : Optional end date
    //   * SearchTerm   : Optional search by order number / keyword
    //   * PageNumber   : Which page to return
    //   * PageSize     : How many records per page

    // What is returned?
    // - PaginatedResultDTO<OrderResponseDTO>, which includes:
    //   * Items      : List of matching orders (current page)
    //   * PageNumber : Current page index
    //   * PageSize   : Page size used
    //   * TotalCount : Total number of matching orders (for pagination UI)

    // How does MediatR use this?
    // - MediatR will send this query to a handler implementing:
    //     IRequestHandler<GetOrdersQuery, PaginatedResultDTO<OrderResponseDTO>>
    // - The handler will:
    //     * Read the Filter.
    //     * Call a repository method like GetOrdersWithFiltersAsync(...)
    //     * Map entities to OrderResponseDTO.
    //     * Wrap them in PaginatedResultDTO and return.
    public class GetOrdersQuery : IRequest<PaginatedResultDTO<OrderResponseDTO>>
    {
        // Encapsulates all filter criteria and pagination settings in a single DTO.
        public OrderFilterRequestDTO Filter { get; }

        // Constructor:
        // Creates a new GetOrdersQuery with the provided filter object.
        // Parameters:
        // filter : An OrderFilterRequestDTO containing all search and pagination parameters.
        public GetOrdersQuery(OrderFilterRequestDTO filter)
        {
            // Ensure the filter is not null.
            // If Filter is null, the handler would not know:
            // - what status to filter by (if any)
            // - what date range to apply
            // - which page to return, etc.
            // So we enforce non-null here to keep the query in a valid state.
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }
    }
}

