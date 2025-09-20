using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cart
{
    public class RemoveCartItemRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "CartItemId is required.")]
        public Guid CartItemId { get; set; }
    }
}
