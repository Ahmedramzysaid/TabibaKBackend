namespace DomainLayer.Models;

public class DigitalPrescriptionItem
{
    public Guid DigitalPrescriptionItemId { get; set; }
    public Guid DigitalPrescriptionId { get; set; }
    public DigitalPrescription DigitalPrescription { get; set; } = null!;

    public string? MedicineName { get; set; }
    public int? PerDay { get; set; }
    public string? Spotlights { get; set; }
}
