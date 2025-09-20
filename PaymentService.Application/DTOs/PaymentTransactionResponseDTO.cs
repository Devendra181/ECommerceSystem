namespace PaymentService.Application.DTOs
{
    public class PaymentTransactionResponseDTO
    {
        public Guid? PaymentId { get; set; }
        public string PaymentStatus { get; set; } = null!;

        public List<TransactionResponseDTO> TransactionResponses { get; set; } = new List<TransactionResponseDTO>();
    }
}
