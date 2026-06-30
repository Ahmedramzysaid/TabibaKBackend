namespace DomainLayer.DTOs;

public class AppointmentWithDetailsDto
{
    public Guid AppointmentID { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string? FormattedDateTime { get; set; }
    public short AppointmentStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public string PatientID { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string? PatientProfileImageUrl { get; set; }

    public string DoctorID { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string? DoctorEmail { get; set; }
    public string? DoctorProfileImageUrl { get; set; }

    public Guid? MedicalRecordId { get; set; }
    public string? VisitDescription { get; set; }
    public string? Diagnosis { get; set; }
    public string? AdditionalNotes { get; set; }

    public int? PaymentID { get; set; }
    public decimal? AmountPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
}
