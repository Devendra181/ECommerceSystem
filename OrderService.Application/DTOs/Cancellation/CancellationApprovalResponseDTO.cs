using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Cancellation
{
    public class CancellationApprovalResponseDTO
    {
        public Guid CancellatioId { get; set; }
        public CancellationStatusEnum Status { get; set; }
        public string? Remarks { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
