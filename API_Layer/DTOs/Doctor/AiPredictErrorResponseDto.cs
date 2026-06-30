using System.Text.Json.Serialization;

namespace ClinicAPI.DTOs;

public class AiPredictErrorResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }
}
