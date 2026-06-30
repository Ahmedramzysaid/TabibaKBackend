using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClinicAPI.Services;

public class AiKeepAliveService : BackgroundService
{
    private static readonly TimeSpan PingInterval = TimeSpan.FromMinutes(2);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiKeepAliveService> _logger;

    public AiKeepAliveService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AiKeepAliveService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔥 AI Keep-Alive started — pinging every {Minutes} min to prevent cold start",
            PingInterval.TotalMinutes);

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PingAsync(stoppingToken);

            try
            {
                await Task.Delay(PingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("🛑 AI Keep-Alive stopped");
    }

    private async Task PingAsync(CancellationToken ct)
    {
        var predictPath = (_configuration["AiSettings:PredictSpecialtyPath"] ?? "predict_specialty").TrimEnd('/');
        if (predictPath.Length > 0 && !predictPath.EndsWith("/"))
            predictPath += "/";

        try
        {
            var client = _httpClientFactory.CreateClient("MedicalSpecialtyAi");

            var body = JsonSerializer.Serialize(new { text = "انا اشعر بالم في الكتف" }, JsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.PostAsync(predictPath, content, ct);
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ AI Keep-Alive ping OK — {Elapsed:F2}s", sw.Elapsed.TotalSeconds);
            }
            else
            {
                _logger.LogWarning("⚠️ AI Keep-Alive ping returned {Status} — endpoint may still be warming up",
                    (int)response.StatusCode);
            }
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("⚠️ AI Keep-Alive ping timed out — endpoint may be cold starting");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("⚠️ AI Keep-Alive ping failed: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AI Keep-Alive ping unexpected error");
        }
    }
}
