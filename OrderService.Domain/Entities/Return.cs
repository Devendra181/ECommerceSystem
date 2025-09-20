using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    [Index(nameof(ReturnStatusId))]
    public class Return
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public int ReturnStatusId { get; set; }
        public ReturnStatus? ReturnStatus { get; set; }
        public int ReasonId { get; set; }
        public ReasonMaster? Reason { get; set; }
        public bool IsPartial { get; set; }
        public Guid UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseTotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RestockingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRefundableAmount { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? ApprovalRemarks { get; set; }
        public string? RejectionRemarks { get; set; }
        public int? ReturnPolicyId { get; set; }
        public ReturnPolicy? Policy { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation property to ReturnItems for partial returns
        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }
}
