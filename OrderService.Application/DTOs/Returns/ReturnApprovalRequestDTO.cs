using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class ReturnApprovalRequestDTO
    {
        [Required(ErrorMessage = "ReturnRequestId is required.")]
        public Guid ReturnRequestId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public ReturnStatusEnum Status { get; set; }

        [MaxLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        public string? ProcessedBy { get; set; }
    }
}
