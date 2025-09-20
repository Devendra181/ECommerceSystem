using OrderService.Application.DTOs.Common;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Order
{
    public class OrderFilterRequestDTO : PaginationDTO
    {
        public Guid? UserId { get; set; }
        public OrderStatusEnum? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; } // order number, email etc.
    }
}
