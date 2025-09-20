using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.MasterEntities;

namespace PaymentService.Domain.Entities
{
    [Index(nameof(OrderId), Name = "IX_Payment_OrderId")]
    [Index(nameof(UserId), Name = "IX_Payment_UserId")]
    [Index(nameof(PaymentStatusId), Name = "IX_Payment_PaymentStatusId")]
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int PaymentMethodTypeId { get; set; }

        [ForeignKey(nameof(PaymentMethodTypeId))]
        public virtual PaymentMethodTypeMaster PaymentMethodTypeMaster { get; set; } = null!;

        public Guid? UserPaymentMethodId { get; set; }

        [ForeignKey(nameof(UserPaymentMethodId))]
        public virtual UserPaymentMethod? UserPaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = null!;

        [Required]
        public int RetryCount { get; set; } = 0;

        [Required]
        public int PaymentStatusId { get; set; }

        [ForeignKey(nameof(PaymentStatusId))]
        public virtual PaymentStatusMaster PaymentStatus { get; set; } = null!;

        [StringLength(1000)]
        public string? PaymentUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual ICollection<GatewayResponse> GatewayResponses { get; set; } = new List<GatewayResponse>();
    }
}
