using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ClinicAPI.Extensions;

public static class HttpContextExtensions
{
    public static string GetClientIpAddress(this HttpContext context)
    {
        if (context == null) return "Unknown";

        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }
        }

        if (context.Request.Headers.ContainsKey("CF-Connecting-IP"))
        {
            var cloudflareIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(cloudflareIp))
            {
                return cloudflareIp.Trim();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
