namespace DomainLayer.DTOs;

public class AppointmentMinimalDto
{
    public Guid AppointmentID { get; set; }
    public string PatientID { get; set; } = string.Empty;
    public string DoctorID { get; set; } = string.Empty;
}
