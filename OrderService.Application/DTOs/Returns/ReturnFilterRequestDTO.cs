using OrderService.Application.DTOs.Common;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Returns
{
    public class ReturnFilterRequestDTO : PaginationDTO
    {
        public Guid? UserId { get; set; }
        public ReturnStatusEnum? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }
}
