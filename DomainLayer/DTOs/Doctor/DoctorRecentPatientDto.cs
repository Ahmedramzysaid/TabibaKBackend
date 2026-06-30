namespace DomainLayer.DTOs;

public class DoctorRecentPatientDto
{
    public string PatientID { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int? PatientAge { get; set; }
    public string PatientType { get; set; } = string.Empty; // "Follow-up", "New Patient"
    public string? NextAppointmentFormatted { get; set; } // e.g. "Tomorrow, 10:00 AM"
    public DateOnly? NextAppointmentDate { get; set; }
    public TimeSpan? NextAppointmentTime { get; set; }
}
