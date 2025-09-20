using OrderService.Domain.Enums;
namespace OrderService.Application.DTOs.Order
{
    public class OrderStatusHistoryResponseDTO
    {
        public Guid Id { get; set; }
        public OrderStatusEnum OldStatus { get; set; }
        public OrderStatusEnum NewStatus { get; set; }
        public DateTime ChangedOn { get; set; }
        public string? Remarks { get; set; }
    }
}
