using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Models;
using FluentValidation;

namespace BusinessLayer.Validations
{
    public class PatientValidator : AbstractValidator<PatientDto>
    {
        public PatientValidator(GeneralEnum.SaveMode mode)
        {
            ApplyValidations(mode);
        }

        public void ApplyValidations(GeneralEnum.SaveMode mode)
        {
            RuleFor(p => p.FullName)
                .NotNull().WithMessage("Full Name is required.")
                .NotEmpty().WithMessage("Full Name cannot be empty.");

            RuleFor(p => p.DateOfBirth)
                .Must(date => date.HasValue && date.Value < DateTime.Now.AddYears(-18))
                .WithMessage("Patient must be at least 18 years old.")
                .When(p => p.DateOfBirth.HasValue);

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            if (mode == GeneralEnum.SaveMode.Add)
            {
                RuleFor(p => p.Id)
                    .Empty().WithMessage("New patient should not have an ID.");
            }
        }
    }
}
