using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class CreateTestingWithFileDto
{
    [Required(ErrorMessage = "Medical record ID is required")]
    public Guid MedicalRecordId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date exam is required")]
    [RegularExpression(@"^(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])-\d{4}$", 
        ErrorMessage = "Date exam must be in MM-dd-yyyy format")]
    public string DateExam { get; set; } = string.Empty; // Format: MM-dd-yyyy

    public IFormFile? File { get; set; } // Optional file upload
}
