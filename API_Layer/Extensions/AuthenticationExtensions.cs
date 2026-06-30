using System.Security.Claims;
using System.Text;
using ClinicAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ClinicAPI.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtOptions = configuration.GetSection("Jwt");
            services.Configure<JwtOptions>(JwtOptions);

            var Jwt = configuration.GetSection("Jwt").Get<JwtOptions>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = Jwt.Issuer,
                        ValidAudience = Jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.Key)),
                        ClockSkew = TimeSpan.Zero,
                        
                        RoleClaimType = "role",
                        NameClaimType = ClaimTypes.Name
                    };
                    
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                                context.Token = accessToken;
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            if (context.Principal?.Identity is not ClaimsIdentity existing)
                            {
                                return Task.CompletedTask;
                            }
                            const string roleType = "role";
                            var nameType = context.Options.TokenValidationParameters.NameClaimType ?? ClaimTypes.Name;
                            var claims = existing.Claims.ToList();
                            var roleValues = claims
                                .Where(c => c.Type == roleType || c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .Where(v => !string.IsNullOrWhiteSpace(v))
                                .Distinct()
                                .ToList();
                            foreach (var roleValue in roleValues)
                            {
                                if (!claims.Any(c => c.Type == roleType && c.Value == roleValue))
                                    claims.Add(new Claim(roleType, roleValue));
                            }
                            var identity = new ClaimsIdentity(
                                claims,
                                existing.AuthenticationType ?? "Bearer",
                                nameType,
                                roleType);
                            context.Principal = new ClaimsPrincipal(identity);
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
