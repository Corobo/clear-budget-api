using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Shared.Auth.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddStandardJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var clientId = jwtConfig["ClientId"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtConfig["Authority"];
                options.Audience = jwtConfig["Audience"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var resourceAccess = context.Principal.FindFirst("resource_access")?.Value;
                            if (resourceAccess != null)
                            {
                                var parsed = JObject.Parse(resourceAccess);
                                var appRoles = parsed[clientId]?["roles"];

                                if (appRoles is JArray roles)
                                {
                                    foreach (var role in roles)
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, role!.ToString()));
                                    }
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
