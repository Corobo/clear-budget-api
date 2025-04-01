using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Authentication;
using CategoriesService.Repositories.Data;
using Microsoft.AspNetCore.Identity;

namespace CategoriesService.Tests.Integration.Postgres;

public class PostgresWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PostgresWebAppFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CategoriesDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<CategoriesDbContext>(options =>
                options.UseNpgsql(_connectionString));

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestingAuthHandler>("Test", _ => { });

            services.Configure<ClaimsIdentityOptions>(options =>
            {
                options.RoleClaimType = "roles"; 
            });

        });
    }


}
