using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace ClinicAPI.Middlewares
{
    public class GlobalErrorHandling : IExceptionHandler
    {
        private readonly ILogger<GlobalErrorHandling> _logger;

        public GlobalErrorHandling(ILogger<GlobalErrorHandling> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("UniqueId", "Request Failed"))
            {
                _logger.LogCritical(exception, "Exception: {Message}\nStackTrace: {StackTrace}", 
                    exception.Message, exception.StackTrace);
            }

            var isDevelopment = httpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>()
                .IsDevelopment();

            var details = new ProblemDetails()
            {
                Title = "An error occurred while processing your request.",
                Detail = isDevelopment 
                    ? $"Error: {exception.Message}\n\nStackTrace: {exception.StackTrace}\n\nInner Exception: {exception.InnerException?.Message}"
                    : "An unexpected error occurred. Please try again later.",
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.com/{500}",
                Status = (int)StatusCodes.Status500InternalServerError
            };

            var response = JsonSerializer.Serialize(details);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(response, cancellationToken);
            return true;
        }
    }
}
