using BusinessLayer.Mapping;
using ClinicAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load("../.env");
builder.Configuration.AddEnvironmentVariables();
builder.UseSerilogRequestLogging();
builder.Services
    .AddApiServices()
    .AddDbServices(builder.Configuration)
    .AddIdentityServices(builder.Configuration)
    .AddAuthorizationPolicies()
    .AddAppServices(builder.Configuration)
    .AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>())
    .AddLoggingService();

var app = builder.Build();

app = await app.UseApiConfiguration();

await app.RunAsync();
