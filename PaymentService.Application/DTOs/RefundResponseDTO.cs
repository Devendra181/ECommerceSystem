namespace PaymentService.Application.DTOs
{
    // DTO for refund response
    public class RefundResponseDTO
    {
        public Guid RefundId { get; set; }
        public string Status { get; set; } = null!;
        public decimal RefundAmount { get; set; }
        public string RefundMethod { get; set; } = null!;
    }
}
