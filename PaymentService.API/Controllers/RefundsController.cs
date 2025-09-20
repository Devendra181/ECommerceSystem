using Microsoft.AspNetCore.Mvc;
using PaymentService.API.DTOs;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;

namespace PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefundsController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundsController> _logger;

        public RefundsController(IRefundService refundService, ILogger<RefundsController> logger)
        {
            _refundService = refundService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<RefundResponseDTO>>> Initiate([FromBody] RefundRequestDTO request)
        {
            try
            {
                var result = await _refundService.InitiateRefundAsync(request);
                return Ok(ApiResponse<RefundResponseDTO>.SuccessResponse(result, "Refund initiated."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Refund validation failed");
                return BadRequest(ApiResponse<RefundResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Refund initiation resource not found");
                return NotFound(ApiResponse<RefundResponseDTO>.FailResponse("Not found", new List<string> { ex.Message }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during refund initiation");
                return Conflict(ApiResponse<RefundResponseDTO>.FailResponse("Invalid operation", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during refund initiation");
                return StatusCode(500, ApiResponse<RefundResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("{refundId:guid}/status")]
        public async Task<ActionResult<ApiResponse<RefundResponseDTO>>> GetStatus([FromRoute] Guid refundId)
        {
            try
            {
                var result = await _refundService.GetRefundStatusAsync(refundId);
                if (result is null)
                    return NotFound(ApiResponse<RefundResponseDTO>.FailResponse("Refund not found"));
                return Ok(ApiResponse<RefundResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching refund status");
                return StatusCode(500, ApiResponse<RefundResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<RefundQueryResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<RefundQueryResponseDTO>>> Search([FromBody] RefundFilterRequestDTO request, CancellationToken ct)
        {
            try
            {
                var data = await _refundService.SearchRefundsAsync(request);
                return Ok(ApiResponse<RefundQueryResponseDTO>.SuccessResponse(data));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed for refund search");
                return BadRequest(ApiResponse<RefundQueryResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during refund search");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<RefundQueryResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("manual-process")]
        public async Task<ActionResult<ApiResponse<ManualRefundProcessResponseDTO>>> ProcessManualRefund([FromBody] ManualRefundProcessRequestDTO request)
        {
            try
            {
                var result = await _refundService.ProcessRefundManuallyAsync(request);
                return Ok(ApiResponse<ManualRefundProcessResponseDTO>.SuccessResponse(result, "Manual refund processed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed for manual refund processing");
                return BadRequest(ApiResponse<ManualRefundProcessResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Refund not found for manual processing");
                return NotFound(ApiResponse<ManualRefundProcessResponseDTO>.FailResponse("Not found", new List<string> { ex.Message }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during manual refund processing");
                return Conflict(ApiResponse<ManualRefundProcessResponseDTO>.FailResponse("Invalid operation", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during manual refund processing");
                return StatusCode(500, ApiResponse<ManualRefundProcessResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("process/pending-refunds")]
        public async Task<ActionResult<ApiResponse<int>>> ProcessPendingRefunds()
        {
            try
            {
                var count = await _refundService.ProcessPendingRefundsAsync();
                return Ok(ApiResponse<int>.SuccessResponse(count, "Pending refunds processed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending refunds");
                return StatusCode(500, ApiResponse<int>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("{refundId:guid}/transactions")]
        public async Task<ActionResult<ApiResponse<RefundTransactionResponseDTO>>> GetTransactions([FromRoute] Guid refundId)
        {
            try
            {
                var data = await _refundService.GetRefundTransactionsAsync(refundId);
                if (data == null)
                    return BadRequest(ApiResponse<RefundTransactionResponseDTO>.FailResponse("Invalid payload", null, null));
                return Ok(ApiResponse<RefundTransactionResponseDTO>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for refund {RefundId}", refundId);
                return StatusCode(500, ApiResponse<RefundTransactionResponseDTO>.FailResponse("Internal server error"));
            }
        }
    }
}
