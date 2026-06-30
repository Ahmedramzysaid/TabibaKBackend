using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class MedicalRecordValidator : AbstractValidator<MedicalRecordDto>
{
    public MedicalRecordValidator()
    {
        ApplyValidations();
    }

    private void ApplyValidations()
    {
        RuleFor(m => m.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");
    }
}
