using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class CreatePatientDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(150, ErrorMessage = "Full name must not exceed 150 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(256)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [MaxLength(256, ErrorMessage = "Password must not exceed 256 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{6,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&#)")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [MaxLength(50, ErrorMessage = "Gender must not exceed 50 characters")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [RegularExpression(@"^(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])-\d{4}$", ErrorMessage = "Date of birth must be in MM-dd-yyyy format")]
    public string DateOfBirth { get; set; } = string.Empty;

    [Required(ErrorMessage = "Latitude is required")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    public double Longitude { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
