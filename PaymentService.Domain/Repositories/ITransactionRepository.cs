using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactionsByPaymentIdAsync(Guid paymentId);
        Task<IEnumerable<Transaction>> GetTransactionsByRefundIdAsync(Guid refundId);
    }
}
