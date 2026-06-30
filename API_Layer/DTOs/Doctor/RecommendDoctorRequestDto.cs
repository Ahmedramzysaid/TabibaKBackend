using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ClinicAPI.DTOs;

public class RecommendDoctorRequestDto
{
    [JsonPropertyName("text")]
    [DefaultValue("headache and fever")]
    public string? Text { get; set; }
}
