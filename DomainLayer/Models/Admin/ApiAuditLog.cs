using System;

namespace DomainLayer.Models;

public class ApiAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = "Unknown";
    public string? UserId { get; set; } // Nullable if the request was anonymous
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string? UserAgent { get; set; }
}
