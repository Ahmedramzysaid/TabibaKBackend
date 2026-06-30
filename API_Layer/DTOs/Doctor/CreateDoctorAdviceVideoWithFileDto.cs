using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class CreateDoctorAdviceVideoWithFileDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public IFormFile Video { get; set; } = null!;
}
