using OrderService.Domain.Enums;
namespace OrderService.Application.DTOs.Returns
{
    public class ReturnResponseDTO
    {
        public Guid ReturnId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public ReturnStatusEnum Status { get; set; }
        public int ReasonId { get; set; }
        public string ReasonText { get; set; } = null!;
        public string? Remarks { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public List<ReturnItemResponseDTO> ReturnItems { get; set; } = new();
    }
}
