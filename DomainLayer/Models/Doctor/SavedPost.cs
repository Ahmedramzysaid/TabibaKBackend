using System;

namespace DomainLayer.Models
{
    public class SavedPost
    {
        public Guid Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public Guid DoctorAdvicePostId { get; set; }
        public DoctorAdvicePost DoctorAdvicePost { get; set; } = null!;
        
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
