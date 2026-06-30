namespace DomainLayer.Models;

public class Patient
{
    public string Id { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
    public MedicalRecord? MedicalRecord { get; set; }
    
    public ApplicationUser User { get; set; } = null!;
}
