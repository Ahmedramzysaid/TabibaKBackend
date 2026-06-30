namespace DomainLayer.DTOs;

public class AppointmentDto
{
    public Guid AppointmentID { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }

    public string PatientID { get; set; } = string.Empty;
    public string DoctorID { get; set; } = string.Empty;

    public Guid? MedicalRecordId { get; set; }
    public MedicalRecordDto? MedicalRecordDto { get; set; }
    public int? ReminderMinutesBefore { get; set; }
}
