using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "..", "API_Layer"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "API_Layer"),
                Path.Combine(Directory.GetCurrentDirectory(), "API_Layer"),
                Directory.GetCurrentDirectory()
            };

            string? basePath = null;
            foreach (var path in possiblePaths)
            {
                var appsettingsPath = Path.Combine(path, "appsettings.json");
                if (File.Exists(appsettingsPath))
                {
                    basePath = path;
                    break;
                }
            }

            if (basePath == null)
            {
                throw new InvalidOperationException("Could not find appsettings.json file. Please ensure it exists in the API_Layer directory.");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("default") ?? 
                                  configuration.GetConnectionString("Default");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'default' or 'Default' not found in appsettings.json");
            }
            
            optionsBuilder.UseSqlServer(connectionString, x => x.UseNetTopologySuite());

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
