using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Tests.Integration.Postgres
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public WebAppFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TransactionsDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<TransactionsDbContext>(options =>
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
}
