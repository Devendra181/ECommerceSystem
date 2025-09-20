using OrderService.Application.DTOs.Common;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Cancellation
{
    public class CancellationFilterRequestDTO : PaginationDTO
    {
        public Guid? UserId { get; set; }
        public CancellationStatusEnum? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }
}
