using AutoMapper;
using MediatR;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Queries;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Queries
{
    // CQRS Query Handler:
    // Handles the "GetOrderByIdQuery" and returns a single OrderResponseDTO (or null).

    // Responsibility of this handler:
    // - It is the "READ" side implementation for fetching an order by its ID.
    // - It:
    //   1) Receives the query (which contains only OrderId).
    //   2) Uses the repository to load the Order entity from the database.
    //   3) Maps the entity to OrderResponseDTO using AutoMapper.
    //   4) Returns the DTO (or null if no order is found).

    // Note:
    // - This class contains **only** read/query logic, no state changes.
    // - It implements MediatR's IRequestHandler<GetOrderByIdQuery, OrderResponseDTO?>:
    //     - TRequest  = GetOrderByIdQuery
    //     - TResponse = OrderResponseDTO? (nullable: might return null if not found)
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDTO?>
    {
        // Repository abstraction for accessing Order data from the database.
        // - Hides the underlying data access (EF Core, Dapper, etc.).
        // - Provides methods like GetByIdAsync.
        private readonly IOrderRepository _orderRepository;

        // AutoMapper instance used to convert domain entities (Order)
        // into DTOs (OrderResponseDTO) that will be returned to the caller.
        private readonly IMapper _mapper;

        // Constructor:
        // Receives dependencies via Dependency Injection.
        // Parameters:
        // orderRepository : Used to retrieve order data from the database.
        // mapper          : Used to map Order entity -> OrderResponseDTO.
        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            // Ensure the dependencies are not null.
            // If they are null, something is wrong with the DI configuration.
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Handle method:
        // This is where the actual query processing happens.
        // Parameters:
        // query            : The GetOrderByIdQuery containing the OrderId to look up.
        // cancellationToken: Used to cancel the operation if the request is aborted (optional).

        // Returns:
        // - OrderResponseDTO? :
        //     * A fully populated DTO if the order exists.
        //     * null if no order was found with the given ID.
        public async Task<OrderResponseDTO?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
        {
            // 1. Use the repository to fetch the order entity by ID.
            //    - This is a pure read operation.
            var order = await _orderRepository.GetByIdAsync(query.OrderId);

            // 2. If no order is found, return null.
            //    - The caller (service/controller) can decide how to handle this:
            //      * return 404 Not Found
            //      * or some custom error response.
            if (order == null)
                return null;

            // 3. Map the domain entity to a DTO using AutoMapper.
            //    - This ensures that API consumers do not directly see domain entities.
            var orderDto = _mapper.Map<OrderResponseDTO>(order);

            // 4. Return the DTO to the caller.
            return orderDto;
        }
    }
}
