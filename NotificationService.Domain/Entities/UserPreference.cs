using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace NotificationService.Domain.Entities
{
    // One preference record per user
    [Index(nameof(UserId), IsUnique = true)]
    public class UserPreference
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
        public bool DoNotDisturb { get; set; } = false;
        public int? MaxDailyNotifications { get; set; }  // Null = unlimited
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
