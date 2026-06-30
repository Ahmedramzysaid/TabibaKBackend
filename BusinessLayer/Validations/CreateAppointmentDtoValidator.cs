using DomainLayer.DTOs;
using DomainLayer.Helpers;
using FluentValidation;

namespace BusinessLayer.Validations;

public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentDtoValidator()
    {
        RuleFor(x => x.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required.");

        RuleFor(x => x.AppointmentTime)
            .NotEmpty().WithMessage("Appointment time is required.");

        RuleFor(x => x)
            .Must(x => AppointmentScheduling.IsInFuture(x.AppointmentDate, x.AppointmentTime))
            .WithMessage("Appointment date and time must be in the future (UTC).");

        RuleFor(x => x.AppointmentStatus)
            .InclusiveBetween(0, 4).WithMessage("Appointment status must be between 0 and 4 (0 = Pending).");

        RuleFor(x => x.PatientID)
            .NotEmpty().WithMessage("Patient ID is required.")
            .NotNull().WithMessage("Patient ID cannot be null.");

        RuleFor(x => x.DoctorID)
            .NotEmpty().WithMessage("Doctor ID is required.")
            .NotNull().WithMessage("Doctor ID cannot be null.");

        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.AdditionalNotes));

        RuleFor(x => x.ReminderMinutesBefore)
            .InclusiveBetween(0, 10080)
            .When(x => x.ReminderMinutesBefore.HasValue)
            .WithMessage("Reminder minutes must be between 0 (no reminder) and 10080 (1 week) when provided.");
    }
}
