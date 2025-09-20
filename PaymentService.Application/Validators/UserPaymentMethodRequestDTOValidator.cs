using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators
{
    public class UserPaymentMethodRequestDTOValidator : AbstractValidator<UserPaymentMethodRequestDTO>
    {
        public UserPaymentMethodRequestDTOValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.PaymentMethodType)
                .NotEmpty()
                .WithMessage("Payment method type is required.");

            RuleFor(x => x.MaskedDetails)
                .NotEmpty()
                .WithMessage("Masked card or wallet details are required.")
                .MaximumLength(50)
                .WithMessage("Masked details must not exceed 50 characters.");

            RuleFor(x => x.ExpiryMonth)
                .InclusiveBetween(1, 12)
                .When(x => x.ExpiryMonth.HasValue)
                .WithMessage("Expiry month must be between 1 and 12.");

            RuleFor(x => x.ExpiryYear)
                .InclusiveBetween(2000, 2100)
                .When(x => x.ExpiryYear.HasValue)
                .WithMessage("Expiry year must be between 2000 and 2100.");
        }
    }
}
