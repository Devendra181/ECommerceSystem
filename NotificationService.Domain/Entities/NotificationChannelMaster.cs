using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace NotificationService.Domain.Entities
{
    [Index(nameof(IsActive))]
    public class NotificationChannelMaster
    {
        [Key]
        public int Id { get; set; } // matches enum value

        [Required]
        [MaxLength(50)]
        public NotificationChannelEnum Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Audit columns
        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
