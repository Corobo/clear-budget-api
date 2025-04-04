using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Testing.Factories;
using Shared.Testing.Utils;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Tests.Integration.Factories
{
    public class TransactionsWebAppFactory : CustomWebApplicationFactory<Program>
    {
        public TransactionsWebAppFactory(string connectionString, 
            Action<IWebHostBuilder>? customWebHostBuilder = null)
            : base(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TransactionsDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<TransactionsDbContext>(options =>
                    options.UseNpgsql(connectionString));

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestingAuthHandler>("Test", _ => { });

                services.Configure<ClaimsIdentityOptions>(options =>
                {
                    options.RoleClaimType = "roles";
                });
            },
            customWebHostBuilder: customWebHostBuilder)
        {
        }
    }
}
