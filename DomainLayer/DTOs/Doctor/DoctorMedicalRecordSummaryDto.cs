namespace DomainLayer.DTOs;

public class DoctorMedicalRecordSummaryDto
{
    public Guid MedicalRecordId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string FormattedDateTime { get; set; } = string.Empty;
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
