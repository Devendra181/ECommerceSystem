using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    public class RefundFilterRequestDTO
    {
        public RefundStatusEnum? Status { get; set; }
        public RefundMethodTypeEnum? RefundMethodType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? PaymentId { get; set; }
        public Guid? UserId { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;  // default: first page
        public int PageSize { get; set; } = 10;   // default: 10 records
    }
}
