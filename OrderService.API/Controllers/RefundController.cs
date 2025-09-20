using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Refunds;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
        }

        // GET api/refund/{refundId}
        [HttpGet("{refundId:guid}")]
        public async Task<ActionResult<ApiResponse<RefundResponseDTO>>> GetRefundById(Guid refundId)
        {
            try
            {
                var refund = await _refundService.GetRefundByIdAsync(refundId);
                if (refund == null)
                    return NotFound(ApiResponse<RefundResponseDTO>.FailResponse("Refund not found."));
                return Ok(ApiResponse<RefundResponseDTO>.SuccessResponse(refund));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RefundResponseDTO>.FailResponse("Failed to get refund.", new List<string> { ex.Message }));
            }
        }

        // DELETE api/refund/{refundId}
        [HttpDelete("{refundId:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteRefund(Guid refundId)
        {
            try
            {
                await _refundService.DeleteRefundAsync(refundId);
                return Ok(ApiResponse<string>.SuccessResponse("Success", "Refund deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Failed to delete refund.", new List<string> { ex.Message }));
            }
        }

        // GET api/refund/cancellation/{cancellationId}
        [HttpGet("cancellation/{cancellationId:guid}")]
        public async Task<ActionResult<ApiResponse<RefundResponseDTO>>> GetRefundByCancellationId(Guid cancellationId)
        {
            try
            {
                var refund = await _refundService.GetByCancellationIdAsync(cancellationId);
                if (refund == null)
                    return NotFound(ApiResponse<RefundResponseDTO>.FailResponse("Refund not found for given cancellation."));
                return Ok(ApiResponse<RefundResponseDTO>.SuccessResponse(refund));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RefundResponseDTO>.FailResponse("Failed to get refund by cancellation.", new List<string> { ex.Message }));
            }
        }

        // GET api/refund/return/{returnId}
        [HttpGet("return/{returnId:guid}")]
        public async Task<ActionResult<ApiResponse<RefundResponseDTO>>> GetRefundByReturnId(Guid returnId)
        {
            try
            {
                var refund = await _refundService.GetByReturnIdAsync(returnId);
                if (refund == null)
                    return NotFound(ApiResponse<RefundResponseDTO>.FailResponse("Refund not found for given return."));
                return Ok(ApiResponse<RefundResponseDTO>.SuccessResponse(refund));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RefundResponseDTO>.FailResponse("Failed to get refund by return.", new List<string> { ex.Message }));
            }
        }

        // GET api/refund/order/{orderId}
        [HttpGet("order/{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<RefundResponseDTO>>>> GetRefundsByOrderId(
            Guid orderId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var refunds = await _refundService.GetRefundsByOrderIdAsync(orderId, pageNumber, pageSize);
                return Ok(ApiResponse<PaginatedResultDTO<RefundResponseDTO>>.SuccessResponse(refunds));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<RefundResponseDTO>>.FailResponse("Failed to get refunds by order.", new List<string> { ex.Message }));
            }
        }

        // POST api/refund/filter
        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<RefundResponseDTO>>>> GetRefundsByFilter([FromBody] RefundFilterRequestDTO filter)
        {
            try
            {
                var refunds = await _refundService.GetRefundsAsync(filter);
                return Ok(ApiResponse<PaginatedResultDTO<RefundResponseDTO>>.SuccessResponse(refunds));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<RefundResponseDTO>>.FailResponse("Failed to get refunds with filter.", new List<string> { ex.Message }));
            }
        }

        // PUT api/refund/update-status
        [HttpPut("update-status")]
        public async Task<ActionResult<ApiResponse<UpdateRefundStatusResponseDTO>>> UpdateRefundStatus(UpdateRefundStatusRequestDTO request)
        {
            try
            {
                var result = await _refundService.UpdateRefundStatusAsync(request);
                return Ok(ApiResponse<UpdateRefundStatusResponseDTO>.SuccessResponse(result, $"Refund {result.Status} successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Failed to update refund status.", new List<string> { ex.Message }));
            }
        }
    }
}
