using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Security.Claims;

namespace Shared.Auth.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddStandardJwtAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("Jwt");

            var audiences = jwtConfig.GetSection("Audience").Get<string[]>();
            var authority = jwtConfig["Authority"];
            var issuers = jwtConfig.GetSection("Issuers").Get<string[]>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudiences = audiences,
                        ValidateIssuer = true,
                        ValidIssuers = issuers
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            Log.Information("JWT token validated successfully.");

                            if (context.Principal?.Identity is ClaimsIdentity identity)
                            {
                                var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                                if (realmAccess != null)
                                {
                                    var parsed = JObject.Parse(realmAccess);
                                    var realmRoles = parsed["roles"] as JArray;

                                    if (realmRoles != null)
                                    {
                                        foreach (var role in realmRoles)
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, role!.ToString()));
                                        }
                                    }
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Log.Error(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Log.Warning("JWT challenge triggered. Error: {Error}, Description: {ErrorDescription}",
                                context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
