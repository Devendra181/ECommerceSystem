using PaymentService.Application.DTOs;
namespace PaymentService.Application.Interfaces
{
    public interface IUserPaymentMethodService
    {
        Task<IEnumerable<UserPaymentMethodResponseDTO>> GetUserPaymentMethodsAsync(Guid userId);
        Task<UserPaymentMethodResponseDTO> AddUserPaymentMethodAsync(UserPaymentMethodRequestDTO paymentMethodDto);
        Task<UserPaymentMethodResponseDTO> UpdateUserPaymentMethodAsync(Guid paymentMethodId, UserPaymentMethodRequestDTO paymentMethodDto);
        Task<bool> DeactivateUserPaymentMethodAsync(Guid paymentMethodId);
    }
}
