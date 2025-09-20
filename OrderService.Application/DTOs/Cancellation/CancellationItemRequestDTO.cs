using System.ComponentModel.DataAnnotations;
namespace OrderService.Application.DTOs.Cancellation
{
    public class CancellationItemRequestDTO
    {
        [Required(ErrorMessage = "OrderItemId is required.")]
        public Guid OrderItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
