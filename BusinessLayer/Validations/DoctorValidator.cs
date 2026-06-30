using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class DoctorValidator : AbstractValidator<DoctorDto>
{
    public DoctorValidator()
    {
        ApplyValidations();
    }

    public void ApplyValidations()
    {
        RuleFor(p => p.FullName)
            .NotNull().WithMessage("Full Name is required.")
            .NotEmpty().WithMessage("Full Name cannot be empty.")
            .MaximumLength(100).WithMessage($"Full Name does not accept length bigger than 100");

        RuleFor(p => p.DateOfBirth)
            .Must(date => date < DateTime.Now.AddYears(-18))
            .WithMessage("Doctor must be at least 18 years old.")
            .When(p => p.DateOfBirth != default);

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email does not accept length bigger than 100");

        RuleFor(p => p.Specialization)
            .MaximumLength(100).WithMessage($"Specialization does not accept length bigger than 100")
            .NotEmpty().WithMessage("Specialization must not be empty.")
            .NotNull().WithMessage("Specialization must not be null.");

        RuleFor(p => p.IdNo)
            .NotEmpty().WithMessage("ID Number is required.")
            .NotNull().WithMessage("ID Number must not be null.")
            .MaximumLength(100).WithMessage("ID Number must not exceed 100 characters.");

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater.")
            .When(p => p.Price.HasValue);

        RuleFor(p => p.IdImageFrontUrl)
            .MaximumLength(500).WithMessage("ID Image Front URL must not exceed 500 characters.")
            .When(p => !string.IsNullOrEmpty(p.IdImageFrontUrl));

        RuleFor(p => p.IdImageBackUrl)
            .MaximumLength(500).WithMessage("ID Image Back URL must not exceed 500 characters.")
            .When(p => !string.IsNullOrEmpty(p.IdImageBackUrl));
    }
}
