using Microsoft.AspNetCore.Http;

namespace ClinicAPI.DTOs;

public class UpdateDoctorAdviceVideoWithFileDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsPublished { get; set; }
    public IFormFile? Video { get; set; }
}
