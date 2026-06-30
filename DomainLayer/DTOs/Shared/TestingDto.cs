namespace DomainLayer.DTOs;

public class TestingDto
{
    public Guid TestingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DateExam { get; set; } = string.Empty; // Format: MM-dd-yyyy
    public string? FilePath { get; set; } // Path to uploaded file
    public string? FileType { get; set; } // File type (pdf, image/png, image/jpeg, etc.)
    public Guid MedicalRecordId { get; set; }
}
