using PaymentService.Domain.Entities;
namespace PaymentService.Domain.Repositories
{
    public interface IUserPaymentMethodRepository
    {
        Task<IEnumerable<UserPaymentMethod>> GetActivePaymentMethodsByUserAsync(Guid userId);
        Task<UserPaymentMethod> AddPaymentMethodAsync(UserPaymentMethod paymentMethod);
        Task UpdatePaymentMethodAsync(UserPaymentMethod paymentMethod);
        Task DeactivatePaymentMethodAsync(Guid paymentMethodId);
    }
}
