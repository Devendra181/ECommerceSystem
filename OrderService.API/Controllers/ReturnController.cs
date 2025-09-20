using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Returns;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnController : ControllerBase
    {
        private readonly IReturnService _returnService;

        public ReturnController(IReturnService returnService)
        {
            _returnService = returnService ?? throw new ArgumentNullException(nameof(returnService));
        }

        [HttpPost("request")]
        public async Task<ActionResult<ApiResponse<ReturnResponseDTO>>> RequestReturn([FromBody] CreateReturnRequestDTO request)
        {
            try
            {
                var result = await _returnService.CreateReturnRequestAsync(request);
                return Ok(ApiResponse<ReturnResponseDTO>.SuccessResponse(result, "Return request submitted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReturnResponseDTO>.FailResponse("Return Request failed.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{returnId:guid}")]
        public async Task<ActionResult<ApiResponse<ReturnResponseDTO>>> GetReturn(Guid returnId)
        {
            try
            {
                var result = await _returnService.GetReturnByIdAsync(returnId);
                if (result == null)
                    return NotFound(ApiResponse<ReturnResponseDTO>.FailResponse("Return request not found."));
                return Ok(ApiResponse<ReturnResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReturnResponseDTO>.FailResponse("Failed to fetch return.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("order/{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<List<ReturnResponseDTO>>>> GetOrderReturns(Guid orderId)
        {
            try
            {
                var result = await _returnService.GetReturnsByOrderIdAsync(orderId);
                if (result == null)
                    return NotFound(ApiResponse<ReturnResponseDTO>.FailResponse("No Return request found."));
                return Ok(ApiResponse<List<ReturnResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReturnResponseDTO>>.FailResponse("Failed to fetch returns.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<ReturnResponseDTO>>> UpdateReturn([FromBody] UpdateReturnRequestDTO request)
        {
            try
            {
                var result = await _returnService.UpdateReturnAsync(request);
                if (result == null)
                    return NotFound(ApiResponse<ReturnResponseDTO>.FailResponse("Return not found or could not be updated."));
                return Ok(ApiResponse<ReturnResponseDTO>.SuccessResponse(result, "Return updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReturnResponseDTO>.FailResponse("Return update failed.", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{returnId:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteReturn(Guid returnId)
        {
            try
            {
                await _returnService.DeleteReturnAsync(returnId);
                return Ok(ApiResponse<string>.SuccessResponse("Deleted", "Return deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Return deletion failed.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("items/{returnId:guid}")]
        public async Task<ActionResult<ApiResponse<List<ReturnItemResponseDTO>>>> GetReturnItems(Guid returnId)
        {
            try
            {
                var result = await _returnService.GetReturnItemsByReturnIdAsync(returnId);
                return Ok(ApiResponse<List<ReturnItemResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReturnItemResponseDTO>>.FailResponse("Failed to fetch return items.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>>> GetReturnsByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _returnService.GetReturnByUserAsync(userId, pageNumber, pageSize);
                return Ok(ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>.FailResponse("Failed to fetch returns for user.", new List<string> { ex.Message }));
            }
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>>> GetReturnsByFilter([FromBody] ReturnFilterRequestDTO filter)
        {
            try
            {
                var result = await _returnService.GetReturnAsync(filter);
                return Ok(ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResultDTO<ReturnResponseDTO>>.FailResponse("Failed to fetch filtered returns.", new List<string> { ex.Message }));
            }
        }

        //Approve or Reject Return Request
        [HttpPut("process")]
        public async Task<ActionResult<ApiResponse<ReturnApprovalResponseDTO>>> ProcessReturn([FromBody] ReturnApprovalRequestDTO request)
        {
            try
            {
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _returnService.ApproveOrRejectReturnAsync(request, accessToken);
                return Ok(ApiResponse<ReturnApprovalResponseDTO>.SuccessResponse(result, $"Return {result.Status} successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReturnApprovalResponseDTO>.FailResponse("Return processing failed.", new List<string> { ex.Message }));
            }
        }
    }
}
