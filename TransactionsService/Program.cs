using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using TransactionsService.Clients.Impl;
using TransactionsService.Clients;
using TransactionsService.Data;
using TransactionsService.Repositories;
using TransactionsService.Repositories.Data;
using TransactionsService.Repositories.Impl;
using TransactionsService.Services;
using TransactionsService.Services.Impl;
using Polly.Retry;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration;
var jwtConfig = config.GetSection("Jwt");
var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();

// === EF Core ===
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<TransactionsDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
}


// === Services ===
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<ICategoryCache, CategoryCache>();

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Transactions API", Version = "v1" });

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

// === Clients ===
builder.Services.AddHttpClient<ICategoriesClient, CategoriesClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Categories"]);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<TransactionsDbContext>();

    var restart = config.GetValue<bool>("RestartSchema");

    if (app.Environment.IsDevelopment() && restart)
    {
        // Drop and recreate schema
        db.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS transactions CASCADE;");
        db.Database.ExecuteSqlRaw("CREATE SCHEMA transactions;");
        db.Database.Migrate();       // Recreate schema
        SeedData.Initialize(db);
    }
    else if (!app.Environment.IsDevelopment() && app.Environment.EnvironmentName != "Testing")
    {
        db.Database.Migrate();       // Safe update
        if (app.Environment.IsDevelopment())
        {
            SeedData.Initialize(db);
        }
    }
}


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