using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class PaymentValidator : AbstractValidator<PaymentDto>
{
    public PaymentValidator()
    {
        Include(new CompletePaymentValidator());
        
        RuleFor(p => p.PaymentID)
            .GreaterThan(0)
            .When(p => p.PaymentID != 0)
            .WithMessage("Payment ID must be greater than 0 when specified.");
    }
}
