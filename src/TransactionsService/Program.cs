using Shared.Messaging.Configuration;
using Shared.Messaging.Connection;
using Shared.Messaging.EventBus;
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
using System.Security.Claims;
using TransactionsService.Clients;
using TransactionsService.Clients.Impl;
using TransactionsService.Data;
using TransactionsService.Messaging.Consumers;
using TransactionsService.Messaging.Subscriptions;
using TransactionsService.Repositories;
using TransactionsService.Repositories.Data;
using TransactionsService.Repositories.Impl;
using TransactionsService.Services;
using TransactionsService.Services.Impl;
using Shared.Auth.Extensions;
using Shared.Logging.Extensions;
using Shared.Middleware.Extensions;
using Shared.Auth.Impl;
using Shared.Auth;
using Shared.Auth.Clients.Impl;

var builder = WebApplication.CreateBuilder(args);

// === Get configuration from appsettings.json ===
var config = builder.Configuration.
    AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .Build();
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
    builder.Services.AddDbContext<TransactionsDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
}


// === Services ===
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<ICategoryCache, CategoryCache>();
builder.Services.AddSingleton<IEventBusConsumer<CategoryEvent>, CategoryEventConsumer>();
builder.Services.AddHostedService<CategorySubscriptionService>();
builder.Services.AddTransient<ServiceAuthHandler>();

// === Logging ===
builder.Host.UseSharedSerilog("TransactionsService");


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
}).AddHttpMessageHandler<ServiceAuthHandler>();
builder.Services.AddHttpClient<IAuthTokenClient, AuthTokenClient>();


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
app.UseSharedMiddlewares();
app.MapControllers();

app.Run();

public partial class Program { }