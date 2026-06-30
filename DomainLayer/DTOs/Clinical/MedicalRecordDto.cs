namespace DomainLayer.DTOs;

public class MedicalRecordDto
{
    public Guid MedicalRecordId { get; set; }
    public string PatientId { get; set; } = string.Empty;
}
