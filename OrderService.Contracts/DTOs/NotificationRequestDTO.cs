using OrderService.Contracts.Enums;
namespace OrderService.Contracts.DTOs
{
    public class NotificationRequestDTO
    {
        public Guid UserId { get; set; }
        public NotificationTypeEnum NotificationType { get; set; }
        public string Message { get; set; } = null!;
        public Guid? OrderId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
