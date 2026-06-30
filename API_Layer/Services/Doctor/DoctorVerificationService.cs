using Microsoft.AspNetCore.Hosting;

namespace ClinicAPI.Services;

public class DoctorVerificationService : IDoctorVerificationService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DoctorVerificationService> _logger;
    private Dictionary<string, string>? _verifiedDoctors;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public DoctorVerificationService(IWebHostEnvironment env, ILogger<DoctorVerificationService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<(bool IsVerified, string? Specialization)> VerifyDoctorNameAsync(string arabicName)
    {
        if (string.IsNullOrWhiteSpace(arabicName))
            return (false, null);

        var doctors = await GetVerifiedDoctorsAsync();
        var searchName = arabicName.Trim();
        
        if (doctors.TryGetValue(searchName, out var specialization))
        {
            return (true, specialization);
        }
        
        return (false, null);
    }

    private async Task<Dictionary<string, string>> GetVerifiedDoctorsAsync()
    {
        if (_verifiedDoctors != null)
            return _verifiedDoctors;

        await _loadLock.WaitAsync();
        try
        {
            if (_verifiedDoctors != null)
                return _verifiedDoctors;

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var csvPath = Path.Combine(webRoot, "VervicationDoctor", "data_clean.csv");

            if (!File.Exists(csvPath))
            {
                _logger.LogWarning("Doctor verification CSV not found at {Path}. No names will be verified.", csvPath);
                _verifiedDoctors = new Dictionary<string, string>(StringComparer.Ordinal);
                return _verifiedDoctors;
            }

            var doctors = new Dictionary<string, string>(StringComparer.Ordinal);

            using var fileStream = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);

            await reader.ReadLineAsync();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                
                var name = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                var specialization = parts.Length > 1 ? parts[1].Trim() : "General"; // Default if missing

                if (!string.IsNullOrEmpty(name))
                {
                    if (!doctors.ContainsKey(name))
                    {
                        doctors.Add(name, specialization);
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} verified doctors from {Path}", doctors.Count, csvPath);
            _verifiedDoctors = doctors;
            return _verifiedDoctors;
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
