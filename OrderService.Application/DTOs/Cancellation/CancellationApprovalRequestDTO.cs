using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cancellation
{
    public class CancellationApprovalRequestDTO
    {
        [Required(ErrorMessage = "CancellationId is required.")]
        public Guid CancellationId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public CancellationStatusEnum Status { get; set; }

        [MaxLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }
        public string? ProcessedBy { get; set; }
    }
}
