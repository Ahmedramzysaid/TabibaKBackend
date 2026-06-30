namespace DomainLayer.Models;

public class DigitalPrescription
{
    public Guid DigitalPrescriptionId { get; set; }
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<DigitalPrescriptionItem> Items { get; set; } = new List<DigitalPrescriptionItem>();
}
