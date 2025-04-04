using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Testing.Factories;
using Shared.Testing.Utils;
using CategoriesService.Repositories.Data;
using Microsoft.AspNetCore.Hosting;

namespace CategoriesService.Tests.Integration.Factories
{
    public class CategoriesWebAppFactory : CustomWebApplicationFactory<Program>
    {
        public CategoriesWebAppFactory(
            string connectionString,
            Action<IWebHostBuilder>? customWebHostBuilder = null)
            : base(
                overrideServices: services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CategoriesDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<CategoriesDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestingAuthHandler>("Test", _ => { });

                    services.Configure<ClaimsIdentityOptions>(options =>
                    {
                        options.RoleClaimType = "roles";
                    });

                    services.PostConfigureAll<AuthenticationOptions>(opts =>
                    {
                        opts.DefaultAuthenticateScheme = "Test";
                        opts.DefaultChallengeScheme = "Test";
                    });
                },
                customWebHostBuilder: customWebHostBuilder
            )
        {
        }
    }

}
