using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Common;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> CreateOrder([FromBody] CreateOrderRequestDTO request)
        {
            try
            {
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _orderService.CreateOrderAsync(request, accessToken);
                return Ok(ApiResponse<OrderResponseDTO>.SuccessResponse(result, "Order placed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OrderResponseDTO>.FailResponse("Order creation failed.", new List<string> { ex.Message }));
            }
        }

        [HttpPost("confirm/{orderId}")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmOrder(Guid orderId)
        {
            try
            {
                // Assuming accessToken from Authorization header
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var success = await _orderService.ConfirmOrderAsync(orderId, accessToken);

                if (!success)
                    return BadRequest(ApiResponse<bool>.FailResponse("Failed to confirm order."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order confirmed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> GetOrder(Guid orderId)
        {
            try
            {
                var result = await _orderService.GetOrderByIdAsync(orderId);
                if (result == null)
                    return NotFound(ApiResponse<OrderResponseDTO>.FailResponse("Order not found."));
                return Ok(ApiResponse<OrderResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OrderResponseDTO>.FailResponse("Failed to fetch order.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<OrderResponseDTO>>>> GetOrdersByUser(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _orderService.GetOrdersByUserAsync(userId, pageNumber, pageSize);
                return Ok(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.FailResponse(ex.Message));
            }
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<OrderResponseDTO>>>> GetOrders([FromBody] OrderFilterRequestDTO filter)
        {
            try
            {
                var result = await _orderService.GetOrdersAsync(filter);
                return Ok(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<OrderResponseDTO>>.FailResponse("Failed to fetch orders.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("change-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangeOrderStatus([FromBody] ChangeOrderStatusRequestDTO request)
        {
            try
            {
                var result = await _orderService.ChangeOrderStatusAsync(request);
                if (!result.Success)
                    return BadRequest(ApiResponse<bool>.FailResponse("Failed to change order status."));
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order status updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<OrderStatusHistoryResponseDTO>>.FailResponse("Failed to change order status", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{orderId:guid}/status-history")]
        public async Task<ActionResult<ApiResponse<List<OrderStatusHistoryResponseDTO>>>> GetStatusHistory(Guid orderId)
        {
            try
            {
                var result = await _orderService.GetOrderStatusHistoryAsync(orderId);
                return Ok(ApiResponse<List<OrderStatusHistoryResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<OrderStatusHistoryResponseDTO>>.FailResponse("Failed to fetch status history.", new List<string> { ex.Message }));
            }
        }
    }
}
