using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators
{
    public class RefundRequestDTOValidator : AbstractValidator<RefundRequestDTO>
    {
        public RefundRequestDTOValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("PaymentId is required.");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0)
                .WithMessage("Refund amount must be greater than zero.");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Refund reason is required.")
                .MaximumLength(1000)
                .WithMessage("Refund reason must not exceed 1000 characters.");

            RuleFor(x => x.InitiatedBy)
                .NotEmpty()
                .WithMessage("Initiator information is required.");
        }
    }
}
