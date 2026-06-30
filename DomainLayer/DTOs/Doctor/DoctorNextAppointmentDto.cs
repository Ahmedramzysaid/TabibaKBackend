namespace DomainLayer.DTOs;

public class DoctorNextAppointmentDto
{
    public Guid AppointmentID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? PatientAge { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string FormattedDateTime { get; set; } = string.Empty;
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
