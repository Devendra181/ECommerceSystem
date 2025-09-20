using Microsoft.AspNetCore.Mvc;
using PaymentService.API.DTOs;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<PaymentInitiateResponseDTO>>> Initiate([FromBody] PaymentInitiateRequestDTO request)
        {
            try
            {
                var result = await _paymentService.InitiatePaymentAsync(request);
                return Ok(ApiResponse<PaymentInitiateResponseDTO>.SuccessResponse(result, "Payment initiated."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Payment initiation validation failed");
                return BadRequest(ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment initiation resource not found");
                return NotFound(ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Not found", new List<string> { ex.Message }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during payment initiation");
                return Conflict(ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Invalid operation", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during payment initiation");
                return StatusCode(500, ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("{paymentId:guid}/status")]
        public async Task<ActionResult<ApiResponse<PaymentStatusResponseDTO>>> GetStatus([FromRoute] Guid paymentId)
        {
            try
            {
                var result = await _paymentService.GetPaymentStatusAsync(paymentId);
                if (result is null)
                    return NotFound(ApiResponse<PaymentStatusResponseDTO>.FailResponse("Payment not found"));
                return Ok(ApiResponse<PaymentStatusResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment status");
                return StatusCode(500, ApiResponse<PaymentStatusResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("retry")]
        public async Task<ActionResult<ApiResponse<PaymentInitiateResponseDTO>>> Retry(RetryPaymentRequestDTO retryPaymentRequestDTO)
        {
            try
            {
                var response = await _paymentService.RetryPaymentAsync(retryPaymentRequestDTO.PaymentId, retryPaymentRequestDTO.PaymentMethodOverride);
                if (response == null)
                    return BadRequest(ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Payment is not eligible for retry", null, null));
                return Ok(ApiResponse<PaymentInitiateResponseDTO>.SuccessResponse(response, "Retry triggered"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid state for retry");
                return Conflict(ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Invalid state", new List<string> { ex.Message }, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment");
                return StatusCode(500, ApiResponse<PaymentInitiateResponseDTO>.FailResponse("Internal server error", null, null));
            }
        }

        [HttpPost("{paymentId:guid}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> Cancel([FromRoute] Guid paymentId)
        {
            try
            {
                var ok = await _paymentService.CancelPaymentAsync(paymentId);
                if (!ok)
                    return NotFound(ApiResponse<bool>.FailResponse("Payment not found", null, false));
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment canceled"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid state for cancel");
                return Conflict(ApiResponse<bool>.FailResponse("Invalid operation", new List<string> { ex.Message }, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment");
                return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error", null, false));
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentStatusResponseDTO>>>> GetPending()
        {
            try
            {
                var data = await _paymentService.GetPendingPaymentsAsync();
                return Ok(ApiResponse<IEnumerable<PaymentStatusResponseDTO>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending payments");
                return StatusCode(500, ApiResponse<IEnumerable<PaymentStatusResponseDTO>>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("process/pending-payments")]
        public async Task<ActionResult<ApiResponse<int>>> ProcessPendingPayments()
        {
            try
            {
                var count = await _paymentService.ProcessPendingPaymentsAsync();
                return Ok(ApiResponse<int>.SuccessResponse(count, "Pending payments processed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending payments");
                return StatusCode(500, ApiResponse<int>.FailResponse("Internal server error"));
            }
        }

        [HttpPost("webhook")]
        public async Task<ActionResult<ApiResponse<bool>>> Webhook([FromBody] PaymentWebhookEventDTO payload)
        {
            try
            {
                var ok = await _paymentService.HandlePaymentWebhookAsync(payload);
                if (!ok)
                    return BadRequest(ApiResponse<bool>.FailResponse("Invalid payload", null, false));
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Webhook processed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment webhook");
                return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error", null, false));
            }
        }

        [HttpGet("{paymentId:guid}/transactions")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionResponseDTO>>> GetTransactions([FromRoute] Guid paymentId)
        {
            try
            {
                var data = await _paymentService.GetPaymentTransactionsAsync(paymentId);
                if (data == null)
                    return BadRequest(ApiResponse<PaymentTransactionResponseDTO>.FailResponse("Invalid payload", null, null));
                return Ok(ApiResponse<PaymentTransactionResponseDTO>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for payment {PaymentId}", paymentId);
                return StatusCode(500, ApiResponse<PaymentTransactionResponseDTO>.FailResponse("Internal server error"));
            }
        }
    }
}

