namespace ClinicAPI.DTOs;

public class DoctorAdviceVideoAcceptedTypesDto
{
    public string[] Extensions { get; set; } = Array.Empty<string>();

    public string AcceptAttribute { get; set; } = string.Empty;

    public string AcceptMimeTypes { get; set; } = string.Empty;

    public long MaxSizeBytes { get; set; }

    public int MaxSizeMB { get; set; }
}
