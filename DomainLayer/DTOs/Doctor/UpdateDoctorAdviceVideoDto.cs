namespace DomainLayer.DTOs;

public class UpdateDoctorAdviceVideoDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsPublished { get; set; }
    public string? VideoUrl { get; set; }
}
