using System.ComponentModel.DataAnnotations;
using DomainLayer.Enums;

namespace DomainLayer.DTOs;

public class CreatePrescriptionDto
{
    [Required(ErrorMessage = "Medical record ID is required")]
    public Guid MedicalRecordId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.StillUnderDoctor; // Default to StillUnderDoctor
}
