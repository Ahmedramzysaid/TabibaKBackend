using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class UpdatePatientDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    public string Id { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "Full name must not exceed 150 characters")]
    public string? FullName { get; set; }

    [MaxLength(256)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [RegularExpression(@"^(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])-\d{4}$", ErrorMessage = "Date of birth must be in MM-dd-yyyy format")]
    public string? DateOfBirth { get; set; }

    [MaxLength(50, ErrorMessage = "Gender must not exceed 50 characters")]
    public string? Gender { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
