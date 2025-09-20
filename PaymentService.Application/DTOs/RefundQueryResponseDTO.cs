namespace PaymentService.Application.DTOs
{
    public class RefundQueryResponseDTO
    {
        public int Total { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<RefundListItemDTO> Items { get; set; } = Array.Empty<RefundListItemDTO>();
    }
}
