using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(NotificationId))]
    [Index(nameof(AttemptedAt))]
    [Index(nameof(StatusId))]
    public class NotificationAttemptLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid NotificationId { get; set; }
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;

        [Required]
        public int AttemptNumber { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        // Track status at this attempt (Pending, Sent, Failed)
        [Required]
        public int StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public NotificationStatusMaster Status { get; set; } = null!;

        // Track channel used for this attempt (Email, SMS, etc.)
        [Required]
        public int ChannelId { get; set; }
        [ForeignKey(nameof(ChannelId))]
        public NotificationChannelMaster Channel { get; set; } = null!;

        public string? ProviderResponse { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
