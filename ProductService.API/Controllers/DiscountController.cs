using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/discounts")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(IDiscountService discountService, ILogger<DiscountController> logger)
        {
            _discountService = discountService;
            _logger = logger;
        }

        [HttpGet("by-product/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            try
            {
                var discounts = await _discountService.GetByProductIdAsync(productId);
                return Ok(ApiResponse<List<DiscountDTO>>.SuccessResponse(discounts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByProductId");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("active/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveDiscountByProductId(Guid productId)
        {
            try
            {
                var discount = await _discountService.GetActiveDiscountByProductIdAsync(productId, DateTime.UtcNow);
                if (discount == null)
                    return NotFound(ApiResponse<string>.FailResponse("No active discount found."));

                return Ok(ApiResponse<DiscountDTO>.SuccessResponse(discount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveDiscountByProductId");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add([FromBody] DiscountCreateDTO createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            try
            {
                var result = await _discountService.AddAsync(createDto);
                if (result == null)
                    return StatusCode(500, ApiResponse<string>.FailResponse("Failed to add discount"));

                return Ok(ApiResponse<DiscountDTO>.SuccessResponse(result, "Discount added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Add");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] DiscountUpdateDTO updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            if (id != updateDto.Id)
                return BadRequest(ApiResponse<string>.FailResponse("ID mismatch"));

            try
            {
                var result = await _discountService.UpdateAsync(updateDto);
                if (result == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Discount with id '{id}' not found"));

                return Ok(ApiResponse<DiscountDTO>.SuccessResponse(result, "Discount updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _discountService.DeleteAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Discount deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }
    }
}
