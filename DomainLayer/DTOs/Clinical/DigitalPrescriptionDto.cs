namespace DomainLayer.DTOs;

public class DigitalPrescriptionDto
{
    public Guid DigitalPrescriptionId { get; set; }
    public Guid MedicalRecordId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<DigitalPrescriptionItemDto> Items { get; set; } = new();
    public string? DoctorName { get; set; }
    public string? Specialist { get; set; }
}
