using System;
using System.Text.Json.Serialization;

namespace DomainLayer.DTOs
{
    public class PatientDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfRegistration { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Email { get; set; }
        public string? ProfileImageUrl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? MedicalRecordId { get; set; }
    }
}
