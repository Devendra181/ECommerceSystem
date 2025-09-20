using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Cancellation;
using OrderService.Application.DTOs.Common;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancellationController : ControllerBase
    {
        private readonly ICancellationService _cancellationService;

        public CancellationController(ICancellationService cancellationService)
        {
            _cancellationService = cancellationService ?? throw new ArgumentNullException(nameof(cancellationService));
        }

        [HttpPost("request")]
        public async Task<ActionResult<ApiResponse<CancellationResponseDTO>>> RequestCancellation([FromBody] CreateCancellationRequestDTO request)
        {
            try
            {
                var result = await _cancellationService.CreateCancellationRequestAsync(request);
                return Ok(ApiResponse<CancellationResponseDTO>.SuccessResponse(result, "Cancellation request submitted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CancellationResponseDTO>.FailResponse("Cancellation failed.", new List<string> { ex.Message }));
            }
        }

        
        [HttpGet("{cancellationId:guid}")]
        public async Task<ActionResult<ApiResponse<CancellationResponseDTO>>> GetCancellationById(Guid cancellationId)
        {
            try
            {
                var result = await _cancellationService.GetCancellationByIdAsync(cancellationId);
                if (result == null)
                    return NotFound(ApiResponse<CancellationResponseDTO>.FailResponse("Cancellation request not found."));
                return Ok(ApiResponse<CancellationResponseDTO>.SuccessResponse(result, "Sucess"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CancellationResponseDTO>.FailResponse("Failed to fetch cancellation.", new List<string> { ex.Message }));
            }
        }

        
        [HttpGet("order/{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<List<CancellationResponseDTO>>>> GetCancellationsByOrderId(Guid orderId)
        {
            try
            {
                var result = await _cancellationService.GetCancellationsByOrderIdAsync(orderId);
                if (result == null)
                    return NotFound(ApiResponse<List<CancellationResponseDTO>>.FailResponse("No Cancellation request found for this order."));
                return Ok(ApiResponse<List<CancellationResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CancellationResponseDTO>>.FailResponse("Failed to fetch cancellations.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<CancellationResponseDTO>>> UpdateCancellation([FromBody] UpdateCancellationRequestDTO request)
        {
            try
            {
                var result = await _cancellationService.UpdateCancellationAsync(request);
                if (result == null)
                    return NotFound(ApiResponse<CancellationResponseDTO>.FailResponse("Cancellation not found or could not be updated."));
                return Ok(ApiResponse<CancellationResponseDTO>.SuccessResponse(result, "Cancellation updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CancellationResponseDTO>.FailResponse("Cancellation update failed.", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{cancellationId:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCancellation(Guid cancellationId)
        {
            try
            {
                await _cancellationService.DeleteCancellationAsync(cancellationId);
                return Ok(ApiResponse<string>.SuccessResponse("Sucess", "Cancellation deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Cancellation deletion failed.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("items/{cancellationId:guid}")]
        public async Task<ActionResult<ApiResponse<List<CancellationItemResponseDTO>>>> GetCancellationItemsByCancellationId(Guid cancellationId)
        {
            try
            {
                var result = await _cancellationService.GetCancellationItemsByCancellationIdAsync(cancellationId);
                return Ok(ApiResponse<List<CancellationItemResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CancellationItemResponseDTO>>.FailResponse("Failed to fetch cancellation items.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>>> GetCancellationsByUserId(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _cancellationService.GetCancellationsByUserAsync(userId, pageNumber, pageSize);
                return Ok(ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>.FailResponse("Failed to fetch cancellations for user.", new List<string> { ex.Message }));
            }
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>>> GetCancellationsByFilter([FromBody] CancellationFilterRequestDTO filter)
        {
            try
            {
                var result = await _cancellationService.GetCancellationsAsync(filter);
                return Ok(ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<CancellationResponseDTO>>.FailResponse("Failed to fetch filtered cancellations.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("process")]
        public async Task<ActionResult<ApiResponse<CancellationApprovalResponseDTO>>> ProcessCancellation([FromBody] CancellationApprovalRequestDTO request)
        {
            try
            {
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _cancellationService.ApproveOrRejectCancellationAsync(request, accessToken);
                return Ok(ApiResponse<CancellationApprovalResponseDTO>.SuccessResponse(result, $"Cancellation {result.Status} successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CancellationApprovalResponseDTO>.FailResponse("Cancellation processing failed.", new List<string> { ex.Message }));
            }
        }
    }
}
