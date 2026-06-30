using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ClinicAPI.Extensions;

public static class SwaggerExtensions
{
    public static Action<SwaggerGenOptions> Options()
    {
        return options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Tabibak ",
                Description =
                    "Tabibak Management is a comprehensive clinic administration system built with ASP.NET Core Web API.\n This system provides essential clinic management operations through a robust RESTful API designed specifically for healthcare administrators.\n\n" +
                    "## 🔐 Authentication\n" +
                    "**Default Admin Credentials:**\n" +
                    "- phone: `1234567890`\n" +
                    "- Password: `StrongPassword123###`\n\n" +
                    "**How to use:**\n" +
                    "1. First, use the `/api/Auth/login` endpoint (no authentication required) to get your JWT token\n" +
                    "2. Copy the **`Token`** value from the login response (PascalCase)\n" +
                    "3. Click the **Authorize** button (🔒) at the top of this page\n" +
                    "4. Paste the JWT only (Swagger adds `Bearer ` automatically) and click Authorize\n" +
                    "5. Now you can use all protected endpoints\n\n" +
                    "**Note:** The `/api/Auth/register` endpoint requires SuperAdmin role. Use the default admin account above.",
                Contact = new OpenApiContact
                {
                    Name = "Tabibak Team",
                    Email = "ramzyis258@gmail.com",
                    Url = new Uri("https://github.com/Ahmedramzysaid")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));


            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT from POST /api/auth/login — paste the Token value only.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });

            options.TagActionsBy(apiDesc =>
            {
                var controllerName = apiDesc.GroupName ?? apiDesc.ActionDescriptor.RouteValues["controller"];
                return new[] { controllerName ?? "Unknown" };
            });

            options.DocumentFilter<OrderedTagsDocumentFilter>();

            options.OrderActionsBy(apiDesc =>
            {
                var httpMethod = apiDesc.HttpMethod ?? "";
                var path = apiDesc.RelativePath ?? "";
                
                var methodOrder = httpMethod switch
                {
                    "GET" => 1,
                    "POST" => 2,
                    "PUT" => 3,
                    "DELETE" => 4,
                    _ => 5
                };
                
                return $"{methodOrder}_{path}";
            });

            options.OperationFilter<DoctorsAiRequestExampleFilter>();
            options.OperationFilter<CreateDoctorFileParametersFilter>();
            options.OperationFilter<BearerAuthOperationFilter>();
        };
    }

    public static Action<SwaggerUIOptions> UiOptions()
    {
        return options =>
        {
            options.DocumentTitle = "Tabibak API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tabibak");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnablePersistAuthorization();
            options.ShowExtensions();
            options.ShowCommonExtensions();
            options.SupportedSubmitMethods(
                SubmitMethod.Get,
                SubmitMethod.Post,
                SubmitMethod.Put,
                SubmitMethod.Delete
            );
        };
    }

    private class OrderedTagsDocumentFilter : IDocumentFilter
    {
        private static readonly Dictionary<string, int> TagOrder = new()
        {
            { "Auth", 1 },
            { "Doctor", 2 },
            { "Patient", 3 },
            { "MedicalRecord", 4 },
            { "Prescription", 5 },
            { "Testing", 6 },
            { "Appointment", 7 },
            { "DoctorsAi", 8 },
            { "Payment", 9 },
            { "Role", 10 },
            { "RoleClaim", 11 },
            { "UserRole", 12 }
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null || swaggerDoc.Tags.Count == 0)
                return;

            var orderedTags = swaggerDoc.Tags
                .Select(tag => new
                {
                    Tag = tag,
                    Order = TagOrder.TryGetValue(tag.Name, out var order) ? order : 99
                })
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Tag.Name)
                .Select(x => x.Tag)
                .ToHashSet();

            swaggerDoc.Tags = orderedTags.ToList();
        }
    }

    private class DoctorsAiRequestExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.RelativePath?.Contains("doctors/ai") != true ||
                context.ApiDescription.HttpMethod != "POST")
                return;

            if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) == true && mediaType != null)
            {
                mediaType.Example = new OpenApiObject
                {
                    ["text"] = new OpenApiString("headache and fever")
                };
            }
        }
    }

    private class CreateDoctorFileParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.RelativePath?.Equals("api/doctor", StringComparison.OrdinalIgnoreCase) != true ||
                context.ApiDescription.HttpMethod != "POST")
                return;

            if (operation.RequestBody?.Content?.TryGetValue("multipart/form-data", out var mediaType) != true || mediaType?.Schema == null)
                return;

            if (mediaType.Schema is not OpenApiSchema concreteSchema)
                return;

            concreteSchema.Properties["IdImageFront"] = new OpenApiSchema { Type = "string", Format = "binary", Description = "ID front image (required)" };
            concreteSchema.Properties["IdImageBack"] = new OpenApiSchema { Type = "string", Format = "binary", Description = "ID back image (required)" };
            concreteSchema.Properties["ProfileImage"] = new OpenApiSchema { Type = "string", Format = "binary", Description = "Optional profile image" };
            if (!concreteSchema.Required.Contains("IdImageFront"))
                concreteSchema.Required.Add("IdImageFront");
            if (!concreteSchema.Required.Contains("IdImageBack"))
                concreteSchema.Required.Add("IdImageBack");
        }
    }

    private class BearerAuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() == true
                || context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

            var hasAllowAnonymous = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
                return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        }
    }
}
