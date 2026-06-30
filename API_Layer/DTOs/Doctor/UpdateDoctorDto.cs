using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class UpdateDoctorDto
{
    [Required(ErrorMessage = "Doctor ID is required")]
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

    [MaxLength(100, ErrorMessage = "Specialization must not exceed 100 characters")]
    public string? Specialization { get; set; }

    [MaxLength(100, ErrorMessage = "ID number must not exceed 100 characters")]
    public string? IdNo { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater")]
    public decimal? Price { get; set; }

    public bool? IsAvailable { get; set; }

    public IFormFile? IdImageFront { get; set; }
    public IFormFile? IdImageBack { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
