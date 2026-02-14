using AutoMapper;
using MediatR;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Queries;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Queries
{
    // CQRS Query Handler:
    // Handles "GetOrdersByUserQuery" and returns a paginated list of orders
    // for a specific user.

    // Responsibility of this handler:
    // - This is the "READ" side for fetching a user's order history.
    // - It:
    //   1) Receives the query (which contains UserId, PageNumber, PageSize).
    //   2) Uses the repository to load the user's orders from the database.
    //   3) Maps the entities to OrderResponseDTO using AutoMapper.
    //   4) Wraps them in a PaginatedResultDTO and returns it.

    // MediatR contract:
    // - Implements IRequestHandler<GetOrdersByUserQuery, PaginatedResultDTO<OrderResponseDTO>>
    //   * TRequest  = GetOrdersByUserQuery
    //   * TResponse = PaginatedResultDTO<OrderResponseDTO>
    public class GetOrdersByUserQueryHandler :
        IRequestHandler<GetOrdersByUserQuery, PaginatedResultDTO<OrderResponseDTO>>
    {
        // Repository abstraction for retrieving orders from the database.
        // - Provides a method like GetByUserIdAsync(userId, pageNumber, pageSize).
        // - Hides the underlying data access implementation (EF Core, Dapper, etc.).
        private readonly IOrderRepository _orderRepository;

        // AutoMapper instance used to map domain entities (Order)
        // to DTOs (OrderResponseDTO) that are safe and convenient for API responses.
        private readonly IMapper _mapper;

        // Constructor:
        // Receives dependencies via Dependency Injection.
        // Parameters:
        // orderRepository : Used to fetch the user's orders from the data store.
        // mapper          : Used to map Order entities to OrderResponseDTO.
        public GetOrdersByUserQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            // Ensure the dependencies are properly injected.
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Handle method:
        // This is where the actual query processing logic lives.
        // Parameters:
        // query            : The GetOrdersByUserQuery containing UserId, PageNumber, and PageSize.
        // cancellationToken: Used to cancel the operation if the client aborts the request (optional).

        // Returns:
        // - PaginatedResultDTO<OrderResponseDTO> containing:
        //   * Items      : List of the user's orders for the requested page.
        //   * PageNumber : Which page we returned.
        //   * PageSize   : How many items per page.
        //   * TotalCount : Total number of orders fetched (for this simple implementation).
        public async Task<PaginatedResultDTO<OrderResponseDTO>> Handle(
            GetOrdersByUserQuery query,
            CancellationToken cancellationToken)
        {
            // 1. Retrieve the orders for the specified user with pagination.
            //    - The repository should apply skip/take based on pageNumber and pageSize.
            var orders = await _orderRepository.GetByUserIdAsync(
                query.UserId,
                query.PageNumber,
                query.PageSize);

            // 2. Map the list of domain entities (Order) to DTOs (OrderResponseDTO).
            //    - Keeps your API layer strongly typed and independent of persistence concerns.
            var orderDtos = _mapper.Map<List<OrderResponseDTO>>(orders);

            // 3. Determine the total count.
            //    - In this simple implementation, we use orders.Count.
            //    - In a real-world scenario, you might have:
            //      * A separate repository method that returns (Items, TotalCount)
            //        so TotalCount represents ALL matching records (not just current page).
            var totalCount = orders.Count;

            // 4. Wrap the DTO list and pagination metadata into PaginatedResultDTO.
            //    - This structure is very useful for building UI pagination controls.
            return new PaginatedResultDTO<OrderResponseDTO>
            {
                Items = orderDtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

