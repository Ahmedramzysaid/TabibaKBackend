using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/doctors/ai")]
[Produces("application/json")]
[Consumes("application/json")]
public class DoctorsAiController : ControllerBase
{
    private const string JsonUtf8ContentType = "application/json; charset=utf-8";

    private static readonly JsonSerializerOptions JsonSnakeCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DoctorsAiController> _logger;

    public DoctorsAiController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<DoctorsAiController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("recommend-by-symptoms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RecommendBySymptoms([FromBody] RecommendDoctorRequestDto? request, CancellationToken cancellationToken)
    {
        var body = new { text = request?.Text ?? string.Empty };
        var requestBody = JsonSerializer.Serialize(body, JsonSnakeCase);

        var predictPath = (_configuration["AiSettings:PredictSpecialtyPath"] ?? "predict_specialty").TrimEnd('/');
        if (predictPath.Length > 0 && !predictPath.EndsWith("/"))
            predictPath += "/";

        try
        {
            var httpClient = _httpClientFactory.CreateClient("MedicalSpecialtyAi");
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResponse = await httpClient.PostAsync(predictPath, content, cancellationToken);

            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)httpResponse.StatusCode;

            return new ContentResult
            {
                Content = responseBody,
                ContentType = JsonUtf8ContentType,
                StatusCode = statusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "error", message = "AI service is currently unavailable" });
        }
    }
}
