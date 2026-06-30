using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class UploadMedicalRecordFileDto
{
    [Required(ErrorMessage = "Medical record ID is required")]
    public Guid MedicalRecordId { get; set; }

    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;
}
