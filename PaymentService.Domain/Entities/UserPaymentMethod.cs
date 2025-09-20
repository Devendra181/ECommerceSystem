using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.MasterEntities;

namespace PaymentService.Domain.Entities
{
    [Index(nameof(UserId), Name = "IX_PaymentMethod_UserId")]
    [Index(nameof(MethodTypeId), Name = "IX_PaymentMethod_MethodTypeId")]
    public class UserPaymentMethod
    {
        [Key]
        public Guid PaymentMethodId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int MethodTypeId { get; set; }

        [ForeignKey(nameof(MethodTypeId))]
        public virtual PaymentMethodTypeMaster MethodType { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string MaskedDetails { get; set; } = null!;  // e.g., last 4 digits of card

        public int? ExpiryMonth { get; set; }  // Nullable for non-card payment methods
        public int? ExpiryYear { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}

