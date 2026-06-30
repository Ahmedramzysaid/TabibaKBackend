namespace DomainLayer.DTOs;

public class CreateAppointmentResponseDto
{
    public Guid AppointmentID { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string Gender { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;

    public Guid? MedicalRecordId { get; set; }
}
