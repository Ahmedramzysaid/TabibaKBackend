namespace DomainLayer.DTOs;

public class DigitalPrescriptionItemDto
{
    public Guid DigitalPrescriptionItemId { get; set; }
    public Guid DigitalPrescriptionId { get; set; }
    public string? MedicineName { get; set; }
    public int? PerDay { get; set; }
    public string? Spotlights { get; set; }
}
