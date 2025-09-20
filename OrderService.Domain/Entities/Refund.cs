using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    [Index(nameof(CancellationId))]
    [Index(nameof(ReturnId))]
    public class Refund
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Guid? CancellationId { get; set; }
        public Cancellation? Cancellation { get; set; }

        public Guid? ReturnId { get; set; }
        public Return? Return { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundAmount { get; set; }

        [MaxLength(100)]
        public string? PaymentMethod { get; set; }
        public DateTime RefundDate { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Remarks { get; set; }
        public int RefundStatusId { get; set; }
        public RefundStatus? RefundStatus { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
