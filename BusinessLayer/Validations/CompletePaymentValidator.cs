using DomainLayer.DTOs;
using FluentValidation;
namespace BusinessLayer.Validations;
public class CompletePaymentValidator : AbstractValidator<CompletePaymentDto>
{
    public CompletePaymentValidator()
    {
        RuleFor(p => p.PaymentDate)
            .NotEmpty()
            .WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Payment date cannot be in the future.");

        RuleFor(p => p.AmountPaid)
            .GreaterThan(0)
            .WithMessage("Amount paid must be greater than 0.");

        RuleFor(p => p.AdditionalNotes)
            .MaximumLength(200)
            .WithMessage("Additional notes cannot exceed 200 characters.")
            .When(p => !string.IsNullOrEmpty(p.AdditionalNotes));
    }
}
