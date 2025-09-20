using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.MasterEntities;

namespace PaymentService.Domain.Entities
{
    [Index(nameof(PaymentId), Name = "IX_Transaction_PaymentId")]
    [Index(nameof(TransactionStatusId), Name = "IX_Transaction_TransactionStatusId")]
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; }

        public Guid? PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment? Payment { get; set; }

        public Guid? RefundId { get; set; }

        [ForeignKey(nameof(RefundId))]
        public virtual Refund? Refund { get; set; }

        [Required]
        public int TransactionStatusId { get; set; }

        [ForeignKey(nameof(TransactionStatusId))]
        public virtual TransactionStatusMaster TransactionStatus { get; set; } = null!;

        [Required]
        public int PaymentProviderConfigurationId { get; set; }

        [ForeignKey(nameof(PaymentProviderConfigurationId))]
        public virtual PaymentProviderConfigurationMaster PaymentProviderConfigurationMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? GatewayTransactionId { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string? PerformedBy { get; set; }

        [Required]
        public int RetryCount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public virtual ICollection<GatewayResponse> GatewayResponses { get; set; } = new HashSet<GatewayResponse>();
    }
}
