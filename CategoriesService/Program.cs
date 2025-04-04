using CategoriesService.Data;
using CategoriesService.Repositories;
using CategoriesService.Repositories.Data;
using CategoriesService.Repositories.Impl;
using CategoriesService.Services;
using CategoriesService.Services.Impl;
using Shared.Messaging.Configuration;
using Shared.Messaging.Connection;
using Shared.Messaging.EventBus;
using Shared.Messaging.EventBus.Impl;
using Shared.Messaging.Events;
using Shared.Messaging.Factories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Serilog;
using Shared.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration;
var jwtConfig = config.GetSection("Jwt");
var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();

// === RabbitMQ ===
builder.Services.Configure<RabbitMQOptions>(
    builder.Configuration.GetSection(RabbitMQOptions.ConfigurationSectionName));
builder.Services.AddSingleton<RabbitMqConnectionAccessor>();
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddHostedService<RabbitMqConnectionInitializer>();



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
builder.Services.AddSingleton<IEventBusProducer<CategoryEvent>, EventBusProducer<CategoryEvent>>();
builder.Services.AddSingleton<Serilog.ILogger>(new LoggerConfiguration().CreateLogger());


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