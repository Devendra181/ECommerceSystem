using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.MasterEntities;

namespace PaymentService.Domain.Entities
{
    [Index(nameof(PaymentId), Name = "IX_Refund_PaymentId")]
    [Index(nameof(PaymentTransactionId), Name = "IX_Refund_TransactionId")]
    [Index(nameof(RefundStatusId), Name = "IX_Refund_RefundStatusId")]
    public class Refund
    {
        [Key]
        public Guid RefundId { get; set; }

        [Required]
        public Guid PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; } = null!;

        [Required]
        public int RefundMethodTypeId { get; set; }

        [ForeignKey(nameof(RefundMethodTypeId))]
        public virtual RefundMethodTypeMaster RefundMethodTypeMaster { get; set; } = null!;

        public Guid? PaymentTransactionId { get; set; }  // Original payment transaction

        [ForeignKey(nameof(PaymentTransactionId))]
        public virtual Transaction? Transaction { get; set; }

        public Guid? RefundTransactionId { get; set; }  // Refund transaction

        [ForeignKey(nameof(RefundTransactionId))]
        public virtual Transaction? RefundTransaction { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? Reason { get; set; }

        [Required]
        public int RefundStatusId { get; set; }

        public int RetryCount { get; set; } = 0;

        [ForeignKey(nameof(RefundStatusId))]
        public virtual RefundStatusMaster RefundStatus { get; set; } = null!;
        public DateTime? ProcessedAt { get; set; }

        [Required]
        public Guid InitiatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<GatewayResponse> GatewayResponses { get; set; } = new List<GatewayResponse>();
    }
}
