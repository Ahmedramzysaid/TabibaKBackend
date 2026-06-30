using System.Diagnostics;
using ClinicAPI.Middlewares;
using DataAccessLayer.Migrations;
using DataAccessLayer.Persistence;
using DataAccessLayer.Seeding;
using DomainLayer.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;

namespace ClinicAPI.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
            options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
            options.JsonSerializerOptions.WriteIndented = true;
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });
        
        services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
        {
            options.LowercaseUrls = true; // Use lowercase URLs for consistency
            options.LowercaseQueryStrings = true;
        });
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(SwaggerExtensions.Options());
        services.AddFluentValidationRulesToSwagger();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        services.AddExceptionHandler<GlobalErrorHandling>();
        services.AddProblemDetails();
        services.AddSignalR();

        services.AddSingleton<ClinicAPI.Services.IDoctorVerificationService, ClinicAPI.Services.DoctorVerificationService>();

        return services;
    }

    public static async Task<WebApplication> UseApiConfiguration(this WebApplication app)
    {

        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("✅ Database migration completed");

                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var connectionString = config.GetConnectionString("default")
                        ?? config.GetConnectionString("Default")
                        ?? config.GetConnectionString("connectionString");
                    await AppointmentGuidMigrationRunner.RunAsync(connectionString ?? "");
                    await PatientBloodTypeWeightHeightRunner.RunAsync(connectionString ?? "");
                    await DigitalPrescriptionItemSpotlightsRunner.RunAsync(connectionString ?? "");
                    await ApplicationUserEmailRequiredUniqueRunner.RunAsync(connectionString ?? "");

                    Console.WriteLine("🌱 Starting database seeding...");
                    var seeder = new PermissionSeeder(dbContext, roleManager);
                    await seeder.SeedAsync();
                    Console.WriteLine("✅ Permissions seeded successfully");

                    Console.WriteLine("👤 Seeding admin user...");
                    await ApplicationUserSeeder.SeedAsync(userManager, roleManager);
                    Console.WriteLine("✅ Admin user seeding completed");

                    Console.WriteLine("🩺 Seeding doctor dashboard test data...");
                    await DoctorDashboardSeeder.SeedAsync(dbContext);
                    Console.WriteLine("✅ Doctor dashboard seeding completed");

                    Console.WriteLine("🩺 Seeding test doctor account...");
                    await TestDoctorSeeder.SeedAsync(userManager, roleManager, dbContext);
                    Console.WriteLine("✅ Test doctor seeding completed");

                    Console.WriteLine("🔄 Updating super admin name (Dr.Sara -> Dr.Ahmed Ramzy)...");
                    await UpdateSuperAdminName.UpdateAsync(userManager);
                    Console.WriteLine("✅ Super admin name update completed");
                }
                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    Console.WriteLine($"⚠️  WARNING: SQL Server error: {sqlEx.Message}");
                    Console.WriteLine("⚠️  Please ensure SQL Server is running and the connection string in appsettings is correct.");
                    Console.WriteLine("⚠️  The application will start but database operations will fail.");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"⚠️  WARNING: Database startup failed: {dbEx.Message}");
                    Console.WriteLine($"⚠️  {dbEx.InnerException?.Message}");
                    Console.WriteLine("⚠️  The application will start but database operations may fail.");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"⚠️  Seeding error: {e.Message}");
            Console.WriteLine($"⚠️  Stack trace: {e.StackTrace}");
            Console.WriteLine($"⚠️  Inner exception: {e.InnerException?.Message}");
            Console.WriteLine("⚠️  Application will continue but database operations may fail.");
        }

        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var medicineDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var medicineConfig = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var medicineLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MedicineExcelSeeder");
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var configuredPath = medicineConfig["MedicinesExcel:Path"]?.Trim();
                string excelPath;
                if (!string.IsNullOrEmpty(configuredPath))
                {
                    excelPath = Path.IsPathRooted(configuredPath)
                        ? configuredPath
                        : Path.GetFullPath(Path.Combine(env.ContentRootPath, configuredPath));
                }
                else
                {
                    var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                    excelPath = Path.Combine(webRoot, "data", "medicines.xlsx");
                }

                await ClinicAPI.Services.MedicineExcelSeeder.SeedAsync(medicineDbContext, excelPath, medicineLogger);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Medicines Excel seed: {ex.Message}. Medicines table may be empty.");
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); // Shows detailed exception page in browser when an unhandled exception occurs
        }
        app.UseExceptionHandler();
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        });

        app.UseMiddleware<LoggingAsyncMiddleware>();
        
        app.UseRouting();
        
        app.UseCors("AllowFrontend");
        
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
            branch =>
            {
                branch.UseAuthentication();
                branch.UseAuthorization();
            });
        
        app.MapControllers();
        app.MapHub<ClinicAPI.Hubs.ChatHub>("/hubs/chat");
        
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(SwaggerExtensions.UiOptions());
        
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                if (ctx.File.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    ctx.File.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    ctx.File.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    ctx.File.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    ctx.File.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                }
            }
        });
        

        return app;
    }
}
