using DomainLayer.Enums;

namespace DomainLayer.DTOs;

public class PrescriptionDto
{
    public Guid PrescriptionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? FilePath { get; set; } // Path to uploaded file (PDF or image)
    public string? FileType { get; set; } // File type (application/pdf, image/png, image/jpeg, etc.)
    public PrescriptionStatus Status { get; set; } // Status: StillUnderDoctor or Finished
    public Guid MedicalRecordId { get; set; }
}
