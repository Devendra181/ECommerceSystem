using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Returns
{
    public class ReturnApprovalResponseDTO
    {
        public Guid ReturnRequestId { get; set; }
        public ReturnStatusEnum Status { get; set; }
        public string? Remarks { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
