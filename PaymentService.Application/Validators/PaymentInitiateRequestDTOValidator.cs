using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators
{
    public class PaymentInitiateRequestDTOValidator : AbstractValidator<PaymentInitiateRequestDTO>
    {
        public PaymentInitiateRequestDTOValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("OrderId is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3, 10)
                .WithMessage("Currency must be between 3 and 10 characters.");

            RuleFor(x => x.PaymentMethodTypeId)
                .NotEmpty()
                .WithMessage("Payment method is required.");
        }
    }
}
