using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Order
{
    public class ChangeOrderStatusResponseDTO
    {
        public Guid OrderId { get; set; }
        public OrderStatusEnum OldStatus { get; set; }
        public OrderStatusEnum NewStatus { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Remarks { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
