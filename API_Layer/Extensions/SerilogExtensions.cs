using Serilog;
using Serilog.Debugging;

namespace ClinicAPI.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder UseSerilogRequestLogging(this WebApplicationBuilder builder)
    {
        SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine($"[Serilog SelfLog] {msg}"));

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            try
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Serilog] Config setup failed, falling back to Console: {ex.Message}");
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Error)
                    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }
        });
        return builder;
    }
}
