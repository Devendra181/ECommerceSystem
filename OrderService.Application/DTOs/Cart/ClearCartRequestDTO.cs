using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cart
{
    public class ClearCartRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }
    }
}
