using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Interfaces;
using FluentValidation;

namespace BusinessLayer.Validations;

public class CompleteAppointmentValidator : AbstractValidator<CompleteAppointmentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAppointmentValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        RuleFor(a => a.AppointmentID)
            .MustAsync(async (id, cancellation) => await IsAppointmentExist(id))
            .WithMessage("Appointment does not exist.");
        
        RuleFor(a=> a.AppointmentID)
            .MustAsync(async (id, cancellation) => await CheckAppointmentStatus(id))
            .WhenAsync(async(dto,cancellation) => await IsAppointmentExist(dto.AppointmentID))
            .WithMessage("Appointment status is not valid to complete.");
        
        RuleFor(a => a.CompletePrescriptionDto)
            .SetValidator(new CompletePrescriptionValidator());
        
        RuleFor(a => a.CompletePaymentDto)
            .SetValidator(new CompletePaymentValidator());
    }

    private async Task<bool> IsAppointmentExist(Guid appointmentId)
        => await _unitOfWork.Appointments.ExistsAsync(a => a.AppointmentID == appointmentId);

    private async Task<bool> CheckAppointmentStatus(Guid appointmentId)
    {
        var isAllowedToComplete = await _unitOfWork.Appointments
            .ExistsAsync(a => a.AppointmentID == appointmentId
                && a.AppointmentStatus != (int)AppointmentStatus.Completed
                && a.AppointmentStatus != (int)AppointmentStatus.Canceled);
        return isAllowedToComplete;
    }
}
