using DataAccessLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Extensions;

public static class DbServicesExtensions
{
    public static IServiceCollection AddDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("default")
                ?? configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("connectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'default' (or 'Default' / 'connectionString') is missing in appsettings.");
            options.UseSqlServer(connectionString, x => x.UseNetTopologySuite());
        });

        return services;
    }
}
