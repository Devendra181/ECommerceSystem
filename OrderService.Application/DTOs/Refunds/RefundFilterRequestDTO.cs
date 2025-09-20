using OrderService.Application.DTOs.Common;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Refunds
{
    public class RefundFilterRequestDTO : PaginationDTO
    {
        public Guid? UserId { get; set; }
        public RefundStatusEnum? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
