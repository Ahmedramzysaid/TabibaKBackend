namespace DomainLayer.DTOs;

public class CreateOrUpdatePrescriptionDto : CompletePrescriptionDto
{
    public Guid PrescriptionId { get; set; }
    public Guid MedicalRecordId { get; set; }
}
