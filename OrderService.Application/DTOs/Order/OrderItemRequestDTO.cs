using System.ComponentModel.DataAnnotations;
namespace OrderService.Application.DTOs.Order
{
    public class OrderItemRequestDTO
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
