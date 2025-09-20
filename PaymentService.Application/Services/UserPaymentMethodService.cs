using FluentValidation;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Services
{
    public class UserPaymentMethodService : IUserPaymentMethodService
    {
        private readonly IValidator<UserPaymentMethodRequestDTO> _paymentMethodValidator;
        private readonly IUserPaymentMethodRepository _userPaymentMethodRepository;

        public UserPaymentMethodService(
                IValidator<UserPaymentMethodRequestDTO> paymentMethodValidator,
                IUserPaymentMethodRepository userPaymentMethodRepository
            )
        {
            _paymentMethodValidator = paymentMethodValidator;
            _userPaymentMethodRepository = userPaymentMethodRepository;
        }

        public async Task<UserPaymentMethodResponseDTO> AddUserPaymentMethodAsync(UserPaymentMethodRequestDTO dto)
        {
            var validationResult = await _paymentMethodValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"Validation failed: {errors}");
            }

            var entity = new UserPaymentMethod
            {
                UserId = dto.UserId,
                MethodTypeId = (int)dto.PaymentMethodType,
                MaskedDetails = dto.MaskedDetails,
                ExpiryMonth = dto.ExpiryMonth,
                ExpiryYear = dto.ExpiryYear,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var added = await _userPaymentMethodRepository.AddPaymentMethodAsync(entity);

            return new UserPaymentMethodResponseDTO
            {
                PaymentMethodId = added.PaymentMethodId,
                MaskedDetails = added.MaskedDetails,
                ExpiryMonth = added.ExpiryMonth,
                ExpiryYear = added.ExpiryYear,
                PaymentMethodType = ((PaymentMethodTypeEnum)added.MethodTypeId).ToString()
            };
        }

        public async Task<bool> DeactivateUserPaymentMethodAsync(Guid paymentMethodId)
        {
            await _userPaymentMethodRepository.DeactivatePaymentMethodAsync(paymentMethodId);
            return true;
        }

        public async Task<UserPaymentMethodResponseDTO> UpdateUserPaymentMethodAsync(Guid paymentMethodId, UserPaymentMethodRequestDTO dto)
        {
            var validationResult = await _paymentMethodValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"Validation failed: {errors}");
            }

            var entity = new UserPaymentMethod
            {
                PaymentMethodId = paymentMethodId,
                UserId = dto.UserId,
                MethodTypeId = (int)dto.PaymentMethodType,
                MaskedDetails = dto.MaskedDetails,
                ExpiryMonth = dto.ExpiryMonth,
                ExpiryYear = dto.ExpiryYear,
                UpdatedAt = DateTime.UtcNow
            };

            await _userPaymentMethodRepository.UpdatePaymentMethodAsync(entity);

            return new UserPaymentMethodResponseDTO
            {
                PaymentMethodId = entity.PaymentMethodId,
                MaskedDetails = entity.MaskedDetails,
                ExpiryMonth = entity.ExpiryMonth,
                ExpiryYear = entity.ExpiryYear,
                PaymentMethodType = ((PaymentMethodTypeEnum)entity.MethodTypeId).ToString()
            };
        }

        public async Task<IEnumerable<UserPaymentMethodResponseDTO>> GetUserPaymentMethodsAsync(Guid userId)
        {
            var methods = await _userPaymentMethodRepository.GetActivePaymentMethodsByUserAsync(userId);

            return methods.Select(pm => new UserPaymentMethodResponseDTO
            {
                PaymentMethodId = pm.PaymentMethodId,
                MaskedDetails = pm.MaskedDetails,
                ExpiryMonth = pm.ExpiryMonth,
                ExpiryYear = pm.ExpiryYear,
                PaymentMethodType = pm.MethodType.Name.ToString()
            });
        }
    }
}
