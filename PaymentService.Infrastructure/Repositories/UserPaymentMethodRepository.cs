using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Repositories
{
    public class UserPaymentMethodRepository : IUserPaymentMethodRepository
    {
        private readonly PaymentDbContext _context;

        public UserPaymentMethodRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<UserPaymentMethod> AddPaymentMethodAsync(UserPaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException(nameof(paymentMethod));

            paymentMethod.PaymentMethodId = Guid.NewGuid();
            paymentMethod.CreatedAt = DateTime.UtcNow;
            paymentMethod.UpdatedAt = DateTime.UtcNow;

            _context.UserPaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return paymentMethod;
        }

        public async Task<IEnumerable<UserPaymentMethod>> GetActivePaymentMethodsByUserAsync(Guid userId)
        {
            return await _context.UserPaymentMethods
                .AsNoTracking()
                .Include(pm => pm.MethodType)
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .ToListAsync();
        }

        public async Task UpdatePaymentMethodAsync(UserPaymentMethod paymentMethod)
        {
            var existing = await _context.UserPaymentMethods.FindAsync(paymentMethod.PaymentMethodId);
            if (existing == null)
                throw new KeyNotFoundException($"Payment method {paymentMethod.PaymentMethodId} not found.");

            existing.MaskedDetails = paymentMethod.MaskedDetails;
            existing.ExpiryMonth = paymentMethod.ExpiryMonth;
            existing.ExpiryYear = paymentMethod.ExpiryYear;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.UserPaymentMethods.Update(existing);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivatePaymentMethodAsync(Guid paymentMethodId)
        {
            var existing = await _context.UserPaymentMethods.FindAsync(paymentMethodId);
            if (existing == null)
                throw new KeyNotFoundException($"Payment method {paymentMethodId} not found.");

            existing.IsActive = false;
            // _context.UserPaymentMethods.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
