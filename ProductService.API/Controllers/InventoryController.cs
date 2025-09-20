using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("availability/{productId:guid}/{quantity:int}")]
        public async Task<IActionResult> CheckAvailability(Guid productId, int quantity)
        {
            try
            {
                var isAvailable = await _inventoryService.IsStockAvailableAsync(productId, quantity);
                return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAvailability");
                return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] InventoryUpdateDTO inventoryUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InventoryUpdateDTO>.FailResponse("Validation failed.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            try
            {
                await _inventoryService.UpdateStockAsync(inventoryUpdateDto);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Stock updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateStock");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("increase-stock-bulk")]
        public async Task<IActionResult> IncreaseStockBulk([FromBody] List<InventoryUpdateDTO> stockUpdates)
        {
            if (stockUpdates == null || !stockUpdates.Any())
                return BadRequest(ApiResponse<string>.FailResponse("Stock update list cannot be empty."));

            if (stockUpdates.Any(x => x.Quantity <= 0))
                return BadRequest(ApiResponse<string>.FailResponse("All quantities must be positive for increasing stock."));

            try
            {
                await _inventoryService.IncreaseStockBulkAsync(stockUpdates);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Bulk stock increased successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IncreaseStockBulk");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }

        // Bulk Decrease Stock
        [HttpPost("decrease-stock-bulk")]
        public async Task<IActionResult> DecreaseStockBulk([FromBody] List<InventoryUpdateDTO> stockUpdates)
        {
            if (stockUpdates == null || !stockUpdates.Any())
                return BadRequest(ApiResponse<string>.FailResponse("Stock update list cannot be empty."));

            if (stockUpdates.Any(x => x.Quantity <= 0))
                return BadRequest(ApiResponse<string>.FailResponse("All quantities must be positive for decreasing stock."));

            try
            {
                await _inventoryService.DecreaseStockBulkAsync(stockUpdates);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Bulk stock decreased successfully."));
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(ApiResponse<string>.FailResponse(ioe.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DecreaseStockBulk");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("verify-stock")]
        public async Task<IActionResult> VerifyStock([FromBody] List<ProductStockInfoRequestDTO> requestedItems)
        {
            try
            {
                if (requestedItems == null || !requestedItems.Any())
                    return BadRequest(ApiResponse<string>.FailResponse("Request list cannot be empty."));

                var results = await _inventoryService.VerifyStockForProductsAsync(requestedItems);

                return Ok(ApiResponse<List<ProductStockInfoResponseDTO>>.SuccessResponse(results, "Stock verification completed successfully."));
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyStock");
                return StatusCode(500, ApiResponse<string>.FailResponse("An unexpected error occurred. Please try again later."));
            }
        }
    }
}
