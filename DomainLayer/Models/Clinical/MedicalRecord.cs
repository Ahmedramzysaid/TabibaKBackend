namespace DomainLayer.Models;

public class MedicalRecord
{
    public Guid MedicalRecordId { get; set; }
    
    public string PatientId { get; set; } = string.Empty;
    public Patient Patient { get; set; } = null!;
    
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public ICollection<DigitalPrescription> DigitalPrescriptions { get; set; } = new List<DigitalPrescription>();
    
    public ICollection<Testing> Testings { get; set; } = new List<Testing>();
    
    public Appointment? Appointment { get; set; }
}
