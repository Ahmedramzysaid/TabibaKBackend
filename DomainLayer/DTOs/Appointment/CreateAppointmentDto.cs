namespace DomainLayer.DTOs;

public class CreateAppointmentDto
{
    public DateOnly AppointmentDate { get; set; }

    public TimeSpan AppointmentTime { get; set; }

    public int AppointmentStatus { get; set; }
    public string? AdditionalNotes { get; set; }
    public string PatientID { get; set; } = string.Empty;
    public string DoctorID { get; set; } = string.Empty;

    public int? ReminderMinutesBefore { get; set; }
}
