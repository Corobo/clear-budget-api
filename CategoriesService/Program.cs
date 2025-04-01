using CategoriesService.Data;
using CategoriesService.Repositories;
using CategoriesService.Repositories.Data;
using CategoriesService.Repositories.Impl;
using CategoriesService.Services;
using CategoriesService.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration;
var jwtConfig = config.GetSection("Jwt");
var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();

// === EF Core ===
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<CategoriesDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
}


// === Services ===
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Categories API", Version = "v1" });

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


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<CategoriesDbContext>();

    var restart = config.GetValue<bool>("RestartSchema");

    if (app.Environment.IsDevelopment() && restart)
    {
        // Drop and recreate schema
        db.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS categories CASCADE;");
        db.Database.ExecuteSqlRaw("CREATE SCHEMA categories;");
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