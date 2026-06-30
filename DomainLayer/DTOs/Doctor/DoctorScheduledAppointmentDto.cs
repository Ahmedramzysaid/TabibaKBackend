namespace DomainLayer.DTOs;

public class DoctorScheduledAppointmentDto
{
    public Guid AppointmentID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientProfileImageUrl { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string FormattedDateTime { get; set; } = string.Empty;
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
