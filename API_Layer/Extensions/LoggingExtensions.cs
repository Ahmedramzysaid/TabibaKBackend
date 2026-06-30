namespace ClinicAPI.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingService(this IServiceCollection services)
    {
        services.AddTransient<LoggingAsyncMiddleware>();
        return services;
    }
}
