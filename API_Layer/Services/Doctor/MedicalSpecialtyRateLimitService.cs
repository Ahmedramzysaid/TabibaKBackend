using System.Collections.Concurrent;

namespace ClinicAPI.Services;

public interface IMedicalSpecialtyRateLimitService
{
    bool TryAcquire(string clientIdentifier);
}

public class MedicalSpecialtyRateLimitService : IMedicalSpecialtyRateLimitService
{
    private const int MaxRequestsPerMinute = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<string, List<DateTimeOffset>> _perIp = new();

    public bool TryAcquire(string clientIdentifier)
    {
        var key = string.IsNullOrWhiteSpace(clientIdentifier) ? "unknown" : clientIdentifier;
        var now = DateTimeOffset.UtcNow;
        var list = _perIp.GetOrAdd(key, _ => new List<DateTimeOffset>());

        lock (list)
        {
            list.RemoveAll(t => now - t > Window);
            if (list.Count >= MaxRequestsPerMinute)
                return false;
            list.Add(now);
            return true;
        }
    }
}
