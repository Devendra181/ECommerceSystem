using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IMediator mediator, ILogger<OrderController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // CREATE ORDER  (WRITE → COMMAND)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> CreateOrder(
            [FromBody] CreateOrderRequestDTO request)
        {
            _logger.LogInformation("CreateOrder request received for UserId: {UserId}", request.UserId);

            try
            {
                // Extract bearer token for downstream microservice calls
                var accessToken = Request.Headers["Authorization"]
                                     .ToString()
                                     .Replace("Bearer ", "");

                // Send Command to MediatR → CommandHandler will execute logic
                // MediatR routes it to CreateOrderCommandHandler
                var result = await _mediator.Send(new CreateOrderCommand(request, accessToken));

                _logger.LogInformation("Order created successfully for UserId: {UserId}", request.UserId);

                return Ok(ApiResponse<OrderResponseDTO>.SuccessResponse(
                    result, "Order placed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order for UserId: {UserId}", request.UserId);

                return BadRequest(ApiResponse<OrderResponseDTO>.FailResponse(
                    "Order creation failed.",
                    new List<string> { ex.Message }));
            }
        }

        // CONFIRM ORDER  (WRITE → COMMAND)
        [HttpPost("confirm/{orderId}")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmOrder(Guid orderId)
        {
            _logger.LogInformation("ConfirmOrder request received for OrderId: {OrderId}", orderId);

            try
            {
                // Read bearer token for payment service communication
                var accessToken = Request.Headers["Authorization"]
                                     .ToString()
                                     .Replace("Bearer ", "");

                // CQRS Command -> ConfirmOrderCommandHandler
                var isConfirmed = await _mediator.Send(new ConfirmOrderCommand(orderId, accessToken));

                if (!isConfirmed)
                {
                    _logger.LogWarning("Order confirmation failed for OrderId: {OrderId}", orderId);
                    return BadRequest(ApiResponse<bool>.FailResponse("Failed to confirm order."));
                }

                _logger.LogInformation("Order confirmed successfully. OrderId: {OrderId}", orderId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order confirmed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming OrderId: {OrderId}", orderId);

                return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
            }
        }

        // CHANGE ORDER STATUS (WRITE → COMMAND)
        [HttpPut("change-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangeOrderStatus(
            [FromBody] ChangeOrderStatusRequestDTO request)
        {
            _logger.LogInformation(
                "ChangeOrderStatus request received. OrderId: {OrderId}, NewStatus: {Status}",
                request.OrderId, request.NewStatus);

            try
            {
                // CQRS Command -> ChangeOrderStatusCommandHandler
                var result = await _mediator.Send(new ChangeOrderStatusCommand(request));

                if (!result.Success)
                {
                    _logger.LogWarning("Status change failed for OrderId: {OrderId}", request.OrderId);
                    return BadRequest(ApiResponse<bool>.FailResponse("Failed to change order status."));
                }

                _logger.LogInformation("Order status updated successfully. OrderId: {OrderId}", request.OrderId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order status updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error changing status for OrderId: {OrderId}", request.OrderId);

                return BadRequest(ApiResponse<bool>.FailResponse(
                    "Failed to change order status.",
                    new List<string> { ex.Message }));
            }
        }

        // GET ORDER BY ID  (READ → QUERY)
        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> GetOrder(Guid orderId)
        {
            _logger.LogInformation("GetOrderById request received for OrderId: {OrderId}", orderId);

            try
            {
                // CQRS Query -> GetOrderByIdQueryHandler
                var result = await _mediator.Send(new GetOrderByIdQuery(orderId));

                if (result == null)
                {
                    _logger.LogWarning("Order not found. OrderId: {OrderId}", orderId);

                    return NotFound(ApiResponse<OrderResponseDTO>.FailResponse("Order not found."));
                }

                _logger.LogInformation("Order details retrieved successfully. OrderId: {OrderId}", orderId);

                return Ok(ApiResponse<OrderResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch order. OrderId: {OrderId}", orderId);

                return BadRequest(ApiResponse<OrderResponseDTO>.FailResponse(
                    "Failed to fetch order.",
                    new List<string> { ex.Message }));
            }
        }

        // GET ORDERS BY USER WITH PAGINATION (READ → QUERY)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<OrderResponseDTO>>>>
            GetOrdersByUser(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            _logger.LogInformation(
                "GetOrdersByUser request received → UserId: {UserId}, Page: {Page}, Size: {PageSize}",
                userId, pageNumber, pageSize);

            try
            {
                // CQRS Query -> GetOrdersByUserQueryHandler
                var result = await _mediator.Send(
                    new GetOrdersByUserQuery(userId, pageNumber, pageSize));

                _logger.LogInformation("Orders fetched successfully for UserId: {UserId}", userId);

                return Ok(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching orders for UserId: {UserId}", userId);

                return BadRequest(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.FailResponse(ex.Message));
            }
        }

        // FILTERED ORDERS QUERY (READ → QUERY)
        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<OrderResponseDTO>>>>
            GetOrders([FromBody] OrderFilterRequestDTO filter)
        {
            _logger.LogInformation("Filtered orders request received.");

            try
            {
                // CQRS Query -> GetOrdersQueryHandler
                var result = await _mediator.Send(new GetOrdersQuery(filter));

                _logger.LogInformation("Filtered orders retrieved successfully.");

                return Ok(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch filtered orders.");

                return BadRequest(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.FailResponse(
                    "Failed to fetch orders.",
                    new List<string> { ex.Message }));
            }
        }

        // ORDER STATUS HISTORY (READ → QUERY)
        [HttpGet("{orderId:guid}/status-history")]
        public async Task<ActionResult<ApiResponse<List<OrderStatusHistoryResponseDTO>>>>
            GetStatusHistory(Guid orderId)
        {
            _logger.LogInformation("GetOrderStatusHistory request received. OrderId: {OrderId}", orderId);

            try
            {
                // CQRS Query -> GetOrderStatusHistoryQueryHandler
                var result = await _mediator.Send(new GetOrderStatusHistoryQuery(orderId));

                _logger.LogInformation("Order status history retrieved for OrderId: {OrderId}", orderId);

                return Ok(ApiResponse<List<OrderStatusHistoryResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch status history for OrderId: {OrderId}", orderId);

                return BadRequest(ApiResponse<List<OrderStatusHistoryResponseDTO>>.FailResponse(
                    "Failed to fetch status history.",
                    new List<string> { ex.Message }));
            }
        }
    }
}
