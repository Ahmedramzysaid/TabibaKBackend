using DomainLayer.DTOs;
using DomainLayer.Interfaces;
using FluentValidation;

namespace BusinessLayer.Validations;

public class CreateOrUpdatePrescriptionValidator : AbstractValidator<CreateOrUpdatePrescriptionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrUpdatePrescriptionValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        Include(new CompletePrescriptionValidator());

        RuleFor(p => p.PrescriptionId)
            .NotEmpty()
            .When(p => p.PrescriptionId != Guid.Empty)
            .WithMessage("Prescription ID must be valid when specified.");

        RuleFor(p => p.MedicalRecordId)
            .NotEmpty()
            .MustAsync(async (id, cancellation) => await IsMedicalRecordExist(id))
            .WithMessage("Medical Record does not exist.");
    }

    private async Task<bool> IsMedicalRecordExist(Guid medicalRecordId)
    {
        return await _unitOfWork.MedicalRecords.ExistsAsync(m => m.MedicalRecordId == medicalRecordId);
    }
}
