using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using BusinessLayer.Validations;
using ClinicAPI.Repositories;
using ClinicAPI.Services;
using DataAccessLayer.UnitOfWork;
using Microsoft.Extensions.Hosting;
using DomainLayer.DTOs;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Interfaces.ServicesInterfaces;
using FluentValidation;

namespace ClinicAPI.Extensions;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        
        services.AddHttpClient();
        
        var aiBaseUrl = configuration["AiSettings:BaseUrl"] ?? "https://wafaagamal-medical-specialty-ai-v2.hf.space";
        services.AddHttpClient("MedicalSpecialtyAi", client =>
        {
            client.BaseAddress = new Uri(aiBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        });
        
        services.AddSingleton<IMedicalSpecialtyRateLimitService, MedicalSpecialtyRateLimitService>();
        
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IDashboardDoctorService, DashboardDoctorService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IDigitalPrescriptionService, DigitalPrescriptionService>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        services.AddScoped<ITestingService, TestingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRoleClaimService, RoleClaimService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IDoctorAdviceVideoService, DoctorAdviceVideoService>();
        services.AddScoped<IDoctorAdvicePostService, DoctorAdvicePostService>();
        services.AddScoped<IValidator<CreateDoctorAdviceVideoDto>, CreateDoctorAdviceVideoValidator>();
        services.AddScoped<IValidator<UpdateDoctorAdviceVideoDto>, UpdateDoctorAdviceVideoValidator>();
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddMemoryCache();
        services.AddHostedService<AppointmentReminderBackgroundService>();
        services.AddHostedService<AiKeepAliveService>();

        return services;
    }
}
