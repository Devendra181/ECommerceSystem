using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    [Index(nameof(CancellationStatusId))]
    public class Cancellation
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public int CancellationStatusId { get; set; }
        public CancellationStatus? CancellationStatus { get; set; }
        public int ReasonId { get; set; }
        public ReasonMaster? Reason { get; set; }
        public Guid UserId { get; set; }
        public bool IsPartial { get; set; }

        // Who processed/approved/rejected (admin or system)
        public string? ProcessedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? ApprovalRemarks { get; set; }
        public string? RejectionRemarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchaseTotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CancellationCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalRefundAmount { get; set; }

        // Policy snapshot (if needed)
        public int? CancellationPolicyId { get; set; }
        public CancellationPolicy? Policy { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        [MaxLength(1000)]
        public string? Remarks { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public ICollection<CancellationItem> CancellationItems { get; set; } = new List<CancellationItem>();
    }
}
