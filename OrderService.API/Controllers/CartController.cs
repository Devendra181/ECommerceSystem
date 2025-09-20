using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Cart;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        [HttpPost("add-item")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> AddItem([FromBody] AddCartItemRequestDTO request)
        {
            try
            {
                var result = await _cartService.AddItemToCartAsync(request);
                if (result == null)
                    return NotFound(ApiResponse<CartResponseDTO>.FailResponse("Cart not found."));

                return Ok(ApiResponse<CartResponseDTO>.SuccessResponse(result, "Item added to cart successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CartResponseDTO>.FailResponse("Failed to add item.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("update-item")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> UpdateItem([FromBody] UpdateCartItemRequestDTO request)
        {
            try
            {
                var result = await _cartService.UpdateCartItemAsync(request);
                if (result == null)
                    return NotFound(ApiResponse<CartResponseDTO>.FailResponse("Cart not found."));

                return Ok(ApiResponse<CartResponseDTO>.SuccessResponse(result, "Cart item updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CartResponseDTO>.FailResponse("Failed to update item.", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("remove-item")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> RemoveItem([FromBody] RemoveCartItemRequestDTO request)
        {
            try
            {
                var result = await _cartService.RemoveCartItemAsync(request);
                if (result == null)
                    return NotFound(ApiResponse<CartResponseDTO>.FailResponse("Cart not found."));

                return Ok(ApiResponse<CartResponseDTO>.SuccessResponse(result, "Cart item removed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CartResponseDTO>.FailResponse("Failed to remove item.", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<string>>> ClearCart([FromBody] ClearCartRequestDTO request)
        {
            try
            {
                await _cartService.ClearCartAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse("Cart cleared.", "Cart cleared successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Failed to clear cart.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> GetCartItems(Guid userId)
        {
            try
            {
                var result = await _cartService.GetCartItemsAsync(userId);
                if (result == null)
                    return NotFound(ApiResponse<CartResponseDTO>.FailResponse("Cart not found."));

                return Ok(ApiResponse<CartResponseDTO>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CartResponseDTO>.FailResponse("Failed to fetch cart.", new List<string> { ex.Message }));
            }
        }

        [HttpPost("merge")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> MergeCarts([FromQuery] Guid targetUserId, [FromQuery] Guid sourceUserId)
        {
            try
            {
                var result = await _cartService.MergeCartsAsync(targetUserId, sourceUserId);
                if (result == null)
                    return NotFound(ApiResponse<CartResponseDTO>.FailResponse("Cart not found."));

                return Ok(ApiResponse<CartResponseDTO>.SuccessResponse(result, "Carts merged successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CartResponseDTO>.FailResponse("Failed to merge cart.", new List<string> { ex.Message }));
            }
        }
    }
}
