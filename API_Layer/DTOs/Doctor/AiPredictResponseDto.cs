using System.Text.Json.Serialization;

namespace ClinicAPI.DTOs;

public class AiPredictResponseDto
{
    [JsonPropertyName("predicted_specialty")]
    public string? PredictedSpecialty { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("top_candidates")]
    public List<TopCandidateDto>? TopCandidates { get; set; }
}

public class TopCandidateDto
{
    [JsonPropertyName("specialty")]
    public string? Specialty { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
