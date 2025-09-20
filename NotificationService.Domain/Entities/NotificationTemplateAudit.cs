using System.ComponentModel.DataAnnotations;
namespace NotificationService.Domain.Entities
{
    public class NotificationTemplateAudit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TemplateId { get; set; }
        public NotificationTemplate Template { get; set; } = null!;

        [Required, MaxLength(200)]
        public string SubjectTemplate { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int Version { get; set; }
        [Required, MaxLength(100)]
        public string ChangedBy { get; set; } = "System";
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
