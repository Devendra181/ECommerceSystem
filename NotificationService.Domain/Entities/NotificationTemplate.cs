using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(TemplateName))]
    [Index(nameof(ChannelId))]
    [Index(nameof(TypeId))]
    [Index(nameof(IsActive))]
    [Index(nameof(IsDefault))]
    [Index(nameof(EffectiveFrom), nameof(EffectiveTo))]
    public class NotificationTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty; //OrderPlaced_Email, OrderPlaced_SMS

        [Required]
        public int ChannelId { get; set; }
        [ForeignKey(nameof(ChannelId))]
        public NotificationChannelMaster Channel { get; set; } = null!; //Email, SMS, InApp

        [Required]
        public int TypeId { get; set; }
        [ForeignKey(nameof(TypeId))]
        public NotificationTypeMaster Type { get; set; } = null!; //OrderPlaced, PaymentSuccess

        [Required, MaxLength(200)]
        public string SubjectTemplate { get; set; } = string.Empty; // e.g., "Order #{OrderId} Confirmed"

        [Required]
        public string Content { get; set; } = string.Empty;         // e.g., HTML body

        public int Version { get; set; } = 1;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<NotificationTemplateAudit> AuditTrail { get; set; } = new List<NotificationTemplateAudit>();
    }
}
