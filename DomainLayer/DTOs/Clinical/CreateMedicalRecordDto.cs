using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class CreateMedicalRecordDto
{
    [Required(ErrorMessage = "Visit description is required")]
    [MaxLength(200, ErrorMessage = "Visit description must not exceed 200 characters")]
    public string VisitDescription { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Diagnosis must not exceed 200 characters")]
    public string? Diagnosis { get; set; }

    [MaxLength(500, ErrorMessage = "Additional notes must not exceed 500 characters")]
    public string? AdditionalNotes { get; set; }

    [Required(ErrorMessage = "Patient ID is required")]
    public string PatientId { get; set; } = string.Empty;

    public string? DoctorId { get; set; }
}
