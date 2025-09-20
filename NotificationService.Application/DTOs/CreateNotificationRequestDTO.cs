using NotificationService.Domain.Enums;
namespace NotificationService.Application.DTOs
{
    public class CreateNotificationRequestDTO
    {
        public Guid UserId { get; set; }
        public NotificationChannelEnum Channel { get; set; }
        public int TypeId { get; set; }
        public Dictionary<string, object>? TemplateData { get; set; }
        public List<RecipientDTO> Recipients { get; set; } = new();
        public List<AttachmentDTO>? Attachments { get; set; }
        public NotificationPriorityEnum Priority { get; set; } = NotificationPriorityEnum.Normal;
        public DateTime? ScheduledAtUtc { get; set; }
        public int? TemplateVersion { get; set; }
        public string CreatedBy { get; set; } = "System";
    }
}
