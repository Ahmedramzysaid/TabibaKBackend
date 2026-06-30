using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace DomainLayer.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required] [MaxLength(150)] public string FullName { get; set; } = string.Empty;

        public DateTime DateOfRegistration { get; set; }
        [Required] public DateTime DateOfBirth { get; set; }
        [Required] [MaxLength(50)] public string Gender { get; set; } = string.Empty;
        
        [Required] public double Latitude { get; set; }
        [Required] public double Longitude { get; set; }
        public string? ProfileImageUrl { get; set; }

        public Point? Location { get; set; }

        public virtual List<RefreshToken>? RefreshTokens { get; set; } = new();
        
        public virtual Doctor? Doctor { get; set; }
        public virtual Patient? Patient { get; set; }
    }
}
