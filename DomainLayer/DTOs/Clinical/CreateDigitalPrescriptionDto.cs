using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class CreateDigitalPrescriptionDto
{
    [Required]
    public Guid MedicalRecordId { get; set; }
    public List<CreateDigitalPrescriptionItemDto>? Items { get; set; }
}
