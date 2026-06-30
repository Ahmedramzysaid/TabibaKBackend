using System.ComponentModel.DataAnnotations;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class CreatePrescriptionWithFileDto
{
    [Required(ErrorMessage = "Medical record ID is required")]
    public Guid MedicalRecordId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.StillUnderDoctor; // Default to StillUnderDoctor

    public IFormFile? File { get; set; } // Optional file upload (PDF or image)
}
