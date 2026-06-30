namespace DomainLayer.Models;

public class Testing
{
    public Guid TestingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DateExam { get; set; }
    public string? FilePath { get; set; } // Path to uploaded file
    public string? FileType { get; set; } // File type (pdf, image/png, image/jpeg, etc.)

    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
}
