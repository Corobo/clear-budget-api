using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using ReportingService.Clients;
using ReportingService.Clients.Impl;
using ReportingService.Services;
using System.Security.Claims;
using Shared.Auth.Extensions;
using Shared.Logging.Extensions;
using Shared.Middleware.Extensions;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration.
    AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .Build();
var jwtConfig = config.GetSection("Jwt");
var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();


// === Services ===
builder.Services.AddScoped<IReportingService, ReportingService.Services.Impl.ReportingService>();

// === Logging ===
builder.Host.UseSharedSerilog("ReportingService");

// === Controllers ===
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// === JWT Authentication / Authorization ===
builder.Services.AddStandardJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Reporting API", Version = "v1" });

    // Optional: Add support for JWT in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// === Cache ===
builder.Services.AddMemoryCache();

// === Clients ===
builder.Services.AddHttpClient<ITransactionsClient, TransactionsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Transactions"]);
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSharedMiddlewares();
app.MapControllers();

app.Run();

public partial class Program { }