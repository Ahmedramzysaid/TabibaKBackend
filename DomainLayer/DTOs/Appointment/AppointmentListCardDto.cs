namespace DomainLayer.DTOs;

public class AppointmentListCardDto
{
    public Guid AppointmentID { get; set; }
    public string DoctorID { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string? FormattedDateTime { get; set; }
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string PatientID { get; set; } = string.Empty;
}
