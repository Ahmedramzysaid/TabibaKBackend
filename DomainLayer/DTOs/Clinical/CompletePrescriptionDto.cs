using DomainLayer.Enums;

namespace DomainLayer.DTOs;

public class CompletePrescriptionDto
{
    public string Title { get; set; } = string.Empty;
    public string? FilePath { get; set; } // Path to uploaded file (PDF or image)
    public string? FileType { get; set; } // File type (application/pdf, image/png, etc.)
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.StillUnderDoctor;
}
