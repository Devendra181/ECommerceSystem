using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Domain.Entities
{
    [Index(nameof(PaymentId), Name = "IX_GatewayResponse_PaymentId")]
    [Index(nameof(TransactionId), Name = "IX_GatewayResponse_TransactionId")]
    [Index(nameof(RefundId), Name = "IX_GatewayResponse_RefundId")]
    public class GatewayResponse
    {
        [Key]
        public Guid GatewayResponseId { get; set; }

        public Guid? PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment? Payment { get; set; }

        public Guid? RefundId { get; set; }

        [ForeignKey(nameof(RefundId))]
        public virtual Refund? Refund { get; set; }

        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction Transaction { get; set; } = null!;

        [Required]
        public string RawResponse { get; set; } = null!;   // Full raw response payload

        [MaxLength(50)]
        public string? StatusCode { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [Required]
        public DateTime ReceivedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}

