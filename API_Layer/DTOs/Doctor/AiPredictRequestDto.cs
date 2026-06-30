using System.Text.Json.Serialization;

namespace ClinicAPI.DTOs;

public class AiPredictRequestDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
