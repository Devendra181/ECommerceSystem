using System.ComponentModel.DataAnnotations;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Order
{
    public class ChangeOrderStatusRequestDTO
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "New order status is required.")]
        public OrderStatusEnum NewStatus { get; set; } 

        [MaxLength(100, ErrorMessage = "ChangedBy cannot exceed 100 characters.")]
        public string? ChangedBy { get; set; } 

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
