namespace DomainLayer.Models;

public class Appointment
{
    public Guid AppointmentID { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeSpan AppointmentTime { get; set; }

    public short AppointmentStatus { get; set; }
    public string? AdditionalNotes { get; set; }

    public string PatientID { get; set; } = string.Empty;
    public Patient Patient { get; set; } = null!;

    public string DoctorID { get; set; } = string.Empty;
    public Doctor Doctor { get; set; } = null!;

    public Guid? MedicalRecordId { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }

    public int? PaymentID { get; set; }
    public Payment? Payment { get; set; }

    public DateTimeOffset? ReminderAt { get; set; }

    public bool ReminderSent { get; set; }
}
