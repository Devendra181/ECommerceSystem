using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly PaymentDbContext _context;

        public TransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            transaction.TransactionId = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.RetryCount = 0;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByPaymentIdAsync(Guid paymentId)
        {
            return await _context.Transactions
                .Include(t => t.TransactionStatus)
                .Where(t => t.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByRefundIdAsync(Guid refundId)
        {
            return await _context.Transactions
                .Include(t => t.TransactionStatus)
                .Where(t => t.RefundId == refundId)
                .ToListAsync();
        }
    }
}
