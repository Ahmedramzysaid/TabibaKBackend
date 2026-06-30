using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Models;

[Table("RefreshTokens")]
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }

    public DateTime ExpiresOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? RevokedOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

    public bool IsActive => RevokedOn == null && !IsExpired;

    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
}
