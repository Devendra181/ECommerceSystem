using NotificationService.Domain.Enums;
namespace NotificationService.Application.DTOs
{
    public class NotificationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int ChannelId { get; set; }
        public int TypeId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public int RetryCount { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public NotificationPriorityEnum Priority { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
