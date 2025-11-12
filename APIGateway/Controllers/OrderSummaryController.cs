using APIGateway.DTOs.Common;
using APIGateway.DTOs.OrderSummary;
using APIGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Controllers
{
    // API Gateway controller that exposes an aggregated Order Summary endpoint.
    // Combines data from multiple microservices (Order, User, Product, Payment)
    // into a single unified response for client consumption.
    [ApiController]
    [Route("gateway/order-summary")]
    public class OrderSummaryController : ControllerBase
    {
        private readonly IOrderSummaryAggregator _aggregator;
        private readonly ILogger<OrderSummaryController> _logger;

        // Constructor injection for dependency management.
        public OrderSummaryController(
            IOrderSummaryAggregator aggregator,
            ILogger<OrderSummaryController> logger)
        {
            _aggregator = aggregator;
            _logger = logger;
        }

        // Fetches a fully aggregated order summary for the given <paramref name="orderId"/>.
        // This endpoint is protected by JWT authentication and requires a valid token.
        [Authorize] // Ensures only authenticated users can access this resource
        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<OrderSummaryResponseDTO>>> GetOrderSummary(Guid orderId)
        {
            try
            {
                // Log the start of aggregation with contextual metadata for observability.
                _logger.LogInformation("Aggregating response for OrderId {OrderId}", orderId);

                // Delegate the core orchestration logic to the aggregator service.
                var summary = await _aggregator.GetOrderSummaryAsync(orderId);

                // CASE 1: Order not found in downstream service.
                if (summary is null)
                {
                    // Return a standardized 404 API response using a common response wrapper.
                    return NotFound(
                        ApiResponse<OrderSummaryResponseDTO>.FailResponse(
                            $"Order with id {orderId} not found."));
                }

                // CASE 2: Successful aggregation — return unified summary data.
                return Ok(
                    ApiResponse<OrderSummaryResponseDTO>.SuccessResponse(summary));
            }
            catch (Exception ex)
            {
                // CASE 3: Unexpected runtime failure (e.g., downstream service unavailable).
                // Log error with contextual information for distributed tracing.
                _logger.LogError(ex,
                    "Error while aggregating order summary for OrderId {OrderId}", orderId);

                // Return structured 500 response with user-friendly message.
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<OrderSummaryResponseDTO>.FailResponse(
                        "An unexpected error occurred while retrieving the order summary."));
            }
        }
    }
}
