using AutoMapper;
using MediatR;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Queries;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Queries
{
    // CQRS Query Handler:
    // Handles the "GetOrderStatusHistoryQuery" and returns the full
    // status-change history for a specific order.

    // Responsibility of this handler:
    // - This is the "READ" side for the order's audit trail / timeline.
    // - It:
    //   1) Receives a GetOrderStatusHistoryQuery containing only OrderId.
    //   2) Uses the repository to load all status-history records for that order.
    //   3) Maps those records to OrderStatusHistoryResponseDTO using AutoMapper.
    //   4) Returns the list of DTOs to the caller.

    // MediatR contract:
    // - Implements IRequestHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistoryResponseDTO>>
    //   * TRequest  = GetOrderStatusHistoryQuery
    //   * TResponse = List<OrderStatusHistoryResponseDTO>
    public class GetOrderStatusHistoryQueryHandler :
        IRequestHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistoryResponseDTO>>
    {
        // Repository abstraction for accessing order data (and related status history)
        // from the database.
        // - Exposes GetOrderStatusHistoryAsync(orderId) which returns all status-change
        //   records for a given order.
        private readonly IOrderRepository _orderRepository;

        // AutoMapper instance used to map the domain status-history entities
        // to response DTOs (OrderStatusHistoryResponseDTO).
        private readonly IMapper _mapper;

        // Constructor:
        // Receives dependencies via Dependency Injection.
        // Parameters:
        // orderRepository : Used to fetch the status history from the data store.
        // mapper          : Used to map domain entities to DTOs.
        public GetOrderStatusHistoryQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            // Validate DI wiring: we must have a repository instance.
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

            // Validate DI wiring: we must have an AutoMapper instance.
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Handle method:
        // This is where the actual query logic is implemented.
        // Parameters:
        // query            : The GetOrderStatusHistoryQuery containing the OrderId.
        // cancellationToken: Used to cancel the operation if the client aborts (optional).

        // Returns:
        // - List<OrderStatusHistoryResponseDTO>:
        //   * Each item represents one status change (old status, new status, date, remarks, etc.).
        public async Task<List<OrderStatusHistoryResponseDTO>> Handle(
            GetOrderStatusHistoryQuery query,
            CancellationToken cancellationToken)
        {
            // 1. Retrieve the raw status history records from the repository
            //    based on the OrderId provided in the query.
            var history = await _orderRepository.GetOrderStatusHistoryAsync(query.OrderId);

            // 2. Map the domain entities to a list of DTOs.
            //    - This keeps your API contracts clean and independent from the persistence model.
            var historyDtos = _mapper.Map<List<OrderStatusHistoryResponseDTO>>(history);

            // 3. Return the DTO list back to the caller (service/controller).
            //    - The caller can use this to show an order's status timeline on the UI.
            return historyDtos;
        }
    }
}

