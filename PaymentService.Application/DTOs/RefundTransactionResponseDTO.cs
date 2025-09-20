namespace PaymentService.Application.DTOs
{
    public class RefundTransactionResponseDTO
    {
        public Guid? RefundId { get; set; }
        public string RefundStatus { get; set; } = null!;

        public List<TransactionResponseDTO> TransactionResponses { get; set; } = new List<TransactionResponseDTO>();
    }
}
