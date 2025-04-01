using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportingService.Clients.Impl;
using ReportingService.Clients;
using ReportingService.Services;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration;
var jwtConfig = config.GetSection("Jwt");
var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();


// === Services ===
builder.Services.AddScoped<IReportingService, ReportingService.Services.Impl.ReportingService>();

// === Controllers ===
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// === JWT Authentication ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = jwtConfig["Authority"];
        options.Audience = jwtConfig["Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            RoleClaimType = "roles" // Keycloak puts roles under "roles"
        };
    });

builder.Services.AddAuthorization(); // No custom policies, use [Authorize(Roles = "...")]

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
app.MapControllers();

app.Run();

public partial class Program { }