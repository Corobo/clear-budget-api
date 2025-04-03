using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Load ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// JWT Config
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                        var appRoles = parsed["clear-budget"]?["roles"];

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

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

app.Run();
