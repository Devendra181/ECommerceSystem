using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class UpdateReturnItemDTO
    {
        [Required]
        public Guid ReturnItemId { get; set; } // ReturnItem Id

        [Required]
        public Guid OrderItemId { get; set; } // Order item being returned

        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
