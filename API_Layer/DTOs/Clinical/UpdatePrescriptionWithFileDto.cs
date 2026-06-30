using System.ComponentModel.DataAnnotations;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class UpdatePrescriptionWithFileDto
{
    [Required(ErrorMessage = "Prescription ID is required")]
    public Guid PrescriptionId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.StillUnderDoctor;

    public IFormFile? File { get; set; } // Optional file upload - if provided, will update FilePath and FileType

    public string? ItemsJson { get; set; }
}
