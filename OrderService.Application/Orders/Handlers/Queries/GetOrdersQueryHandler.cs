using AutoMapper;
using MediatR;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Queries;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Queries
{
    // CQRS Query Handler:
    // Handles the "GetOrdersQuery" and returns a paginated, filtered list of orders.

    // Responsibility of this handler:
    // - This is the "READ" side for the admin / back-office order listing.
    // - It:
    //   1) Receives a GetOrdersQuery which contains an OrderFilterRequestDTO (Filter).
    //   2) Uses the filter parameters (status, date range, search term, pagination) to query the repository.
    //   3) Maps the resulting Order entities to OrderResponseDTO.
    //   4) Wraps them in a PaginatedResultDTO and returns to the caller.

    // MediatR contract:
    // - Implements IRequestHandler<GetOrdersQuery, PaginatedResultDTO<OrderResponseDTO>>
    //   * TRequest  = GetOrdersQuery
    //   * TResponse = PaginatedResultDTO<OrderResponseDTO>
    public class GetOrdersQueryHandler :
        IRequestHandler<GetOrdersQuery, PaginatedResultDTO<OrderResponseDTO>>
    {
        // Repository abstraction to access order data in the database.
        // - Exposes a method GetOrdersWithFiltersAsync that:
        //   * Applies status filter
        //   * Applies date range filter
        //   * Applies search term filter (order number, etc.)
        //   * Applies pagination (pageNumber, pageSize)
        //   * Returns (orders, totalCount)
        private readonly IOrderRepository _orderRepository;

        // AutoMapper instance used to map domain entities (Order)
        // to DTOs (OrderResponseDTO) which are safe and convenient for the API layer.
        private readonly IMapper _mapper;

        // Constructor:
        // Injects the required dependencies via Dependency Injection.
        // Parameters:
        // orderRepository : Used to query orders based on supplied filter criteria.
        // mapper          : Used to convert Order entities to OrderResponseDTO.
        public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            // Validate that dependencies are provided by the DI container.
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Handle method:
        // The core logic that executes when a GetOrdersQuery is sent through MediatR.
        // Parameters:
        // query            : Contains the Filter (OrderFilterRequestDTO) with search + pagination criteria.
        // cancellationToken: Used to cancel the operation if the request is aborted (optional).

        // Returns:
        // - PaginatedResultDTO<OrderResponseDTO> containing:
        //   * Items      : List of orders for the current page.
        //   * PageNumber : Current page number from the filter.
        //   * PageSize   : Page size from the filter.
        //   * TotalCount : Total number of orders that match the filter.
        public async Task<PaginatedResultDTO<OrderResponseDTO>> Handle(
            GetOrdersQuery query,
            CancellationToken cancellationToken)
        {
            // 1. Extract and validate the filter from the query.
            //    - The filter contains: Status, FromDate, ToDate, SearchTerm, PageNumber, PageSize, etc.
            //    - If filter is null, we cannot perform a filtered query, so we throw an exception.
            var filter = query.Filter ?? throw new ArgumentNullException(nameof(query.Filter));

            // 2. Call the repository to get orders that match the filter.
            //    - GetOrdersWithFiltersAsync returns a tuple:
            //        (orders: List<Order>, totalCount: int)
            //    - status         : Optional OrderStatusEnum filter.
            //    - fromDate/toDate: Optional date range to limit orders.
            //    - searchOrderNumber: Free-text search for order number / keyword.
            //    - pageNumber/pageSize: For pagination.
            var (orders, totalCount) = await _orderRepository.GetOrdersWithFiltersAsync(
                status: filter.Status,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                searchOrderNumber: filter.SearchTerm,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            // 3. Map the list of Order entities to a list of OrderResponseDTO.
            //    - This decouples the API response model from the persistence model.
            //    - AutoMapper configuration must be set up to map Order -> OrderResponseDTO.
            var orderDtos = _mapper.Map<List<OrderResponseDTO>>(orders);

            // 4. Wrap the resulting DTO list and pagination info into PaginatedResultDTO.
            //    - This DTO is very convenient for building paged grids in the UI:
            //        * Items      : data for the current page
            //        * PageNumber : current page index
            //        * PageSize   : items per page
            //        * TotalCount : total records (used to compute total pages)
            return new PaginatedResultDTO<OrderResponseDTO>
            {
                Items = orderDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
