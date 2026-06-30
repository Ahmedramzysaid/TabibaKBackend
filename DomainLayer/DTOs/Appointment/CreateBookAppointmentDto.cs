namespace DomainLayer.DTOs;

public class CreateBookAppointmentDto
{
    public string PatientID { get; set; } = string.Empty;
    public string DoctorID { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? SpecialtyName { get; set; }
    public string? AppointmentTypeName { get; set; }
}
