namespace DomainLayer.DTOs;

public class DoctorTodayAppointmentItemDto
{
    public Guid AppointmentID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string TimeAndPurpose { get; set; } = string.Empty; // e.g. "10:00 AM - Follow-up"
    public string StatusName { get; set; } = string.Empty; // "Confirmed", "Pending"
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public short AppointmentStatus { get; set; }
}
