using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shared.Auth.Extensions;
using System.Security.Claims;
using Shared.Logging.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load ocelot config
builder.Configuration
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// JWT Config
var jwtConfig = builder.Configuration.GetSection("Jwt");
// === JWT Authentication / Authorization ===
builder.Services.AddStandardJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// === Logging ===
builder.Host.UseSharedSerilog("APIGateway");

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

app.Run();
