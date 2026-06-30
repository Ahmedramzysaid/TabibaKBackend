using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Serilog.Context;
using ClinicAPI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using DataAccessLayer.Persistence;
using DomainLayer.Models;

public class LoggingAsyncMiddleware : IMiddleware
{
    private readonly ILogger<LoggingAsyncMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _maxRequestBodySize;
    private readonly int _maxResponseBodySize;
    private readonly HashSet<string> _sensitiveHeaders;
    private readonly HashSet<string> _sensitiveQueryParams;

    public LoggingAsyncMiddleware(
        ILogger<LoggingAsyncMiddleware> logger,
        IServiceScopeFactory scopeFactory,
        int maxRequestBodySize = 1024 * 10, // 10KB default
        int maxResponseBodySize = 1024 * 10) // 10KB default
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _maxRequestBodySize = maxRequestBodySize;
        _maxResponseBodySize = maxResponseBodySize;

        _sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-API-Key",
            "X-Auth-Token", "Authentication", "Proxy-Authorization"
        };

        _sensitiveQueryParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "token", "apikey", "secret", "key", "access_token"
        };
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = Guid.NewGuid().ToString();

        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        var requestBody = await LogRequestAsync(context, correlationId, startTime);

        var originalResponseBodyStream = context.Response.Body;

        Exception? caughtException = null;
        using var responseBodyStream = new MemoryStream();


        context.Response.Body = responseBodyStream;

        var clientIp = context.GetClientIpAddress();
        
        using (LogContext.PushProperty("UniqueId", correlationId))
        using (LogContext.PushProperty("ClientIP", clientIp))
        {
            await next(context);
        }

        stopwatch.Stop();
        var endTime = DateTime.UtcNow;

        string? responseBody = null;
        if (responseBodyStream.Length > 0)
        {
            responseBodyStream.Position = 0;
            responseBody = await ReadResponseBodyFromStreamAsync(responseBodyStream, context.Response.ContentType);
        }

        await LogResponseAsync(context, correlationId, startTime, endTime,
            stopwatch.ElapsedMilliseconds, responseBody,
            caughtException, requestBody);

        if (responseBodyStream.Length > 0)
        {
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }

        context.Response.Body = originalResponseBodyStream;
    }

    private async Task<string?> LogRequestAsync(HttpContext context, string correlationId, DateTime startTime)
    {
        var request = context.Request;

        var requestBody = await ReadRequestBodyAsync(request);

        var requestInfo = new
        {
            CorrelationId = correlationId,
            Timestamp = startTime,
            request.Method,
            Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            request.Protocol,
            request.ContentType,
            request.ContentLength,

            ClientIP = context.GetClientIpAddress(),
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),

            UserId = context.User?.Identity?.Name,
            IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,

            Headers = FilterSensitiveHeaders(request.Headers),

            QueryParameters = FilterSensitiveQueryParams(request.Query),

            Body = FormatBodyForLogging(requestBody, request.ContentType)
        };

        using (LogContext.PushProperty("UniqueId", correlationId))
        {
            _logger.LogInformation("HTTP Request: {@RequestInfo}", requestInfo);
        }

        context.Items["CorrelationId"] = correlationId;

        return requestBody;
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId,
        DateTime startTime, DateTime endTime, long durationMs,
        string? responseBody, Exception? exception,
        string? requestBody)
    {
        var response = context.Response;

        var responseInfo = new
        {
            CorrelationId = correlationId,
            RequestTimestamp = startTime,
            ResponseTimestamp = endTime,
            DurationMs = durationMs,
            response.StatusCode,
            StatusDescription = GetStatusDescription(response.StatusCode),
            response.ContentType,
            ContentLength = response.ContentLength ?? responseBody?.Length ?? 0,

            Headers = FilterSensitiveHeaders(response.Headers.ToDictionary(h => h.Key, h => h.Value.AsEnumerable())),

            Exception = exception != null
                ? new
                {
                    Type = exception.GetType().Name, exception.Message
                }
                : null,

            PerformanceCategory = ClassifyPerformance(durationMs),

            Body = FormatBodyForLogging(responseBody, response.ContentType),

            RequestBody = FormatBodyForLogging(requestBody, context.Request.ContentType)
        };

        var logLevel = GetLogLevel(response.StatusCode, exception);
        using (LogContext.PushProperty("UniqueId", correlationId))
        {
            _logger.Log(logLevel, "HTTP Response: {@ResponseInfo}", responseInfo);
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var auditLog = new ApiAuditLog
            {
                IpAddress = context.GetClientIpAddress(),
                Endpoint = $"{context.Request.Path}{context.Request.QueryString}",
                Method = context.Request.Method,
                StatusCode = response.StatusCode,
                DurationMs = durationMs,
                UserId = context.User?.Identity?.Name,
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault()
            };
            
            dbContext.ApiAuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save API Audit Log to database.");
        }
    }


    private Dictionary<string, object> FilterSensitiveHeaders(IHeaderDictionary headers)
    {
        return headers
            .Where(h => !_sensitiveHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => (object)h.Value.ToString());
    }

    private Dictionary<string, object> FilterSensitiveHeaders(Dictionary<string, IEnumerable<string?>> headers)
    {
        return headers
            .Where(h => !_sensitiveHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => (object)string.Join(", ", h.Value));
    }

    private Dictionary<string, string> FilterSensitiveQueryParams(IQueryCollection queryParams)
    {
        return queryParams
            .Where(q => !_sensitiveQueryParams.Contains(q.Key))
            .ToDictionary(q => q.Key, q => q.Value.ToString());
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method))
            return null;

        if (request.ContentLength == null || request.ContentLength == 0) return null;

        if (request.ContentLength > _maxRequestBodySize) return $"[Body too large: {request.ContentLength} bytes]";

        try
        {
            request.EnableBuffering();

            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            request.Body.Position = 0;

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading request body");
            return $"[Error reading body: {ex.Message}]";
        }
    }

    private async Task<string?> ReadResponseBodyFromStreamAsync(MemoryStream responseStream, string? contentType)
    {
        if (responseStream.Length == 0) return null;

        if (responseStream.Length > _maxResponseBodySize) return $"[Response too large: {responseStream.Length} bytes]";

        if (!IsReadableContentType(contentType)) return $"[Non-readable content: {contentType}]";

        try
        {
            var originalPosition = responseStream.Position;

            responseStream.Position = 0;

            using var reader = new StreamReader(responseStream, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            responseStream.Position = originalPosition;

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading response body");
            return $"[Error reading response body: {ex.Message}]";
        }
    }

    private string? FormatBodyForLogging(string? body, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        if (IsJsonContent(contentType))
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(body);
                return JsonSerializer.Serialize(jsonDoc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
            }
            catch (JsonException)
            {
                return "[Invalid JSON]";
            }
        }

        if (contentType?.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
            return Uri.UnescapeDataString(body);

        if (contentType?.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
            return "[Multipart/form-data content omitted]";

        return body.Length > _maxRequestBodySize
            ? $"[Truncated Body: {body.Substring(0, _maxRequestBodySize)}...]"
            : body;
    }


    private bool IsJsonContent(string? contentType)
    {
        return !string.IsNullOrEmpty(contentType) &&
               (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("text/json", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsReadableContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;

        var readableTypes = new[]
        {
            "application/json",
            "application/xml",
            "text/xml",
            "text/plain",
            "text/html",
            "application/x-www-form-urlencoded"
        };

        return readableTypes.Any(type =>
            contentType.Contains(type, StringComparison.OrdinalIgnoreCase));
    }

    private string GetStatusDescription(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            _ => $"HTTP {statusCode}"
        };
    }

    private string ClassifyPerformance(long durationMs)
    {
        return durationMs switch
        {
            < 100 => "Fast",
            < 500 => "Normal",
            < 1000 => "Slow",
            < 2000 => "Very Slow",
            _ => "Critical"
        };
    }

    private LogLevel GetLogLevel(int statusCode, Exception? exception)
    {
        if (exception != null) return LogLevel.Error;

        return statusCode switch
        {
            >= 500 => LogLevel.Critical,
            >= 400 => LogLevel.Error,
            _ => LogLevel.Information
        };
    }
}
