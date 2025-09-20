using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cart
{
    public class UpdateCartItemRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "CartItemId is required.")]
        public Guid CartItemId { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
