using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using ReportingService.Clients;
using ReportingService.Clients.Impl;
using ReportingService.Services;
using System.Security.Claims;

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