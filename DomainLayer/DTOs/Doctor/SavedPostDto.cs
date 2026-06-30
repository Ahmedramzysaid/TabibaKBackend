using System;

namespace DomainLayer.DTOs
{
    public class SavedPostDto
    {
        public Guid PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? DoctorName { get; set; }
        public string SavedByUserId { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }
}
