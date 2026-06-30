using DomainLayer.DTOs;
using DomainLayer.Helpers;
using FluentValidation;

namespace BusinessLayer.Validations;

public class AppointmentValidator : AbstractValidator<AppointmentDto>
{
    public AppointmentValidator()
    {
        ApplyValidations();
    }

    private void ApplyValidations()
    {
        RuleFor(a => a.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required.");

        RuleFor(a => a.AppointmentTime)
            .NotEmpty().WithMessage("Appointment time is required.");

        RuleFor(a => a)
            .Must(a => AppointmentScheduling.IsInFuture(a.AppointmentDate, a.AppointmentTime))
            .WithMessage("Appointment date must be in the future.");

        RuleFor(a => a.AppointmentStatus)
            .InclusiveBetween((short)0, (short)4)
            .WithMessage("Appointment status must be between 0 and 4 (0 = Pending).");

        RuleFor(a => a.PatientID)
            .NotEmpty().WithMessage("Patient ID is required.")
            .NotNull().WithMessage("Patient ID cannot be null.");

        RuleFor(a => a.DoctorID)
            .NotEmpty().WithMessage("Doctor ID is required.")
            .NotNull().WithMessage("Doctor ID cannot be null.");

        RuleFor(a => a.MedicalRecordId)
            .NotEmpty()
            .When(a => a.MedicalRecordId.HasValue)
            .WithMessage("Medical Record ID must be valid when specified.");

        When(a => a.MedicalRecordDto != null, () =>
        {
            RuleFor(a => a.MedicalRecordDto)
                .SetValidator(new MedicalRecordValidator());
        });

        RuleFor(a => a.AdditionalNotes).MaximumLength(2000).When(a => !string.IsNullOrEmpty(a.AdditionalNotes));
    }
}
