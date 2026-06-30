namespace DomainLayer.DTOs;

public class CreateDigitalPrescriptionItemDto
{
    public string? MedicineName { get; set; }
    public int? PerDay { get; set; }
    public string? Spotlights { get; set; }
}
