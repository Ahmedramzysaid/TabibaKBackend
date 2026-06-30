using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class CompletePrescriptionValidator : AbstractValidator<CompletePrescriptionDto>
{
    public CompletePrescriptionValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

    }
}
