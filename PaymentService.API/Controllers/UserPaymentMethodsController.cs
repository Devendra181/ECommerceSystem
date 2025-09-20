using Microsoft.AspNetCore.Mvc;
using PaymentService.API.DTOs;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPaymentMethodsController : ControllerBase
    {
        private readonly IUserPaymentMethodService _service;
        private readonly ILogger<UserPaymentMethodsController> _logger;

        public UserPaymentMethodsController(IUserPaymentMethodService service, ILogger<UserPaymentMethodsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserPaymentMethodResponseDTO>>> Add([FromBody] UserPaymentMethodRequestDTO dto)
        {
            try
            {
                var result = await _service.AddUserPaymentMethodAsync(dto);
                return Ok(ApiResponse<UserPaymentMethodResponseDTO>.SuccessResponse(result, "Payment method added."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while adding payment method");
                return BadRequest(ApiResponse<UserPaymentMethodResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method");
                return StatusCode(500, ApiResponse<UserPaymentMethodResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpPut("{paymentMethodId:guid}")]
        public async Task<ActionResult<ApiResponse<UserPaymentMethodResponseDTO>>> Update([FromRoute] Guid paymentMethodId, [FromBody] UserPaymentMethodRequestDTO dto)
        {
            try
            {
                var result = await _service.UpdateUserPaymentMethodAsync(paymentMethodId, dto);
                return Ok(ApiResponse<UserPaymentMethodResponseDTO>.SuccessResponse(result, "Payment method updated."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating payment method");
                return BadRequest(ApiResponse<UserPaymentMethodResponseDTO>.FailResponse("Validation failed", new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment method not found for update");
                return NotFound(ApiResponse<UserPaymentMethodResponseDTO>.FailResponse("Not found", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method");
                return StatusCode(500, ApiResponse<UserPaymentMethodResponseDTO>.FailResponse("Internal server error"));
            }
        }

        [HttpDelete("{paymentMethodId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Deactivate([FromRoute] Guid paymentMethodId)
        {
            try
            {
                var ok = await _service.DeactivateUserPaymentMethodAsync(paymentMethodId);
                return Ok(ApiResponse<bool>.SuccessResponse(ok, ok ? "Payment method deactivated." : null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment method not found for deactivation");
                return NotFound(ApiResponse<bool>.FailResponse("Not found", new List<string> { ex.Message }, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating payment method");
                return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error", null, false));
            }
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserPaymentMethodResponseDTO>>>> GetByUser([FromRoute] Guid userId)
        {
            try
            {
                var data = await _service.GetUserPaymentMethodsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserPaymentMethodResponseDTO>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user payment methods");
                return StatusCode(500, ApiResponse<IEnumerable<UserPaymentMethodResponseDTO>>.FailResponse("Internal server error"));
            }
        }
    }
}
