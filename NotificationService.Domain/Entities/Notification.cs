using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(UserId))]
    [Index(nameof(StatusId))]
    [Index(nameof(ScheduledAt))]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ChannelId { get; set; }
        [ForeignKey(nameof(ChannelId))]
        public NotificationChannelMaster Channel { get; set; } = null!;

        [Required]
        public int TypeId { get; set; }
        [ForeignKey(nameof(TypeId))]
        public NotificationTypeMaster Type { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        // Store dynamic placeholder values 
        [Required]
        public string TemplateData { get; set; } = null!;

        [Required]
        public int StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public NotificationStatusMaster Status { get; set; } = null!;

        public int RetryCount { get; set; } = 0;
        public DateTime? ScheduledAt { get; set; }
        public NotificationPriorityEnum Priority { get; set; } = NotificationPriorityEnum.Normal;
        public string? ErrorMessage { get; set; }

        // Navigation Properties
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
        public ICollection<NotificationAttachment> Attachments { get; set; } = new List<NotificationAttachment>(); public ICollection<NotificationAttemptLog> AttemptLogs { get; set; } = new List<NotificationAttemptLog>();

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
