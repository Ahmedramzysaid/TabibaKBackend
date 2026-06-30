namespace DomainLayer.Models;

public class DoctorRating
{
    public int Id { get; set; }
    public Guid AppointmentID { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string DoctorID { get; set; } = string.Empty;
    public Doctor Doctor { get; set; } = null!;
    public string PatientID { get; set; } = string.Empty;
    public Patient Patient { get; set; } = null!;
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
