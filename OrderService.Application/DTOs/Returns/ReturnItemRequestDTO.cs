using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class ReturnItemRequestDTO
    {
        [Required(ErrorMessage = "OrderItemId is required.")]
        public Guid OrderItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ReturnQuantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
