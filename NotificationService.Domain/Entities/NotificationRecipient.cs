using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(NotificationId))]
    public class NotificationRecipient
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid NotificationId { get; set; }
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public RecipientTypeEnum RecipientType { get; set; } = RecipientTypeEnum.To;

        // Audit Columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
