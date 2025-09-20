using PaymentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.DTOs
{
    public class ManualRefundProcessRequestDTO
    {
        [Required] public Guid RefundId { get; set; }

        // Amount to settle (must equal Refund.RefundAmount for now)
        [Required] public decimal Amount { get; set; }

        // Who performed this manual settlement (staff/admin id)
        [Required] public Guid PerformedBy { get; set; }

        // Bank UTR / Wallet Txn Id / Receipt No, etc.
        [Required, MaxLength(100)]
        public string SettlementReference { get; set; } = default!;

        // Optional: override method type if needed (e.g., BankTransfer, Wallet, Manual)
        public RefundMethodTypeEnum? MethodTypeOverride { get; set; }

        // Optional: when it was actually processed (defaults to UtcNow)
        public DateTime? ProcessedAtUtc { get; set; }

        // Optional free-form notes
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}

