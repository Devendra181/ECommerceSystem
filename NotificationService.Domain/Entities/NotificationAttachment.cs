using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(NotificationId))]
    public class NotificationAttachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid NotificationId { get; set; }
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string MimeType { get; set; } = "application/pdf";

        public long FileSize { get; set; }
        public string StorageType { get; set; } = "FileSystem";

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
