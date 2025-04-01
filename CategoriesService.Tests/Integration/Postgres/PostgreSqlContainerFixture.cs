using Microsoft.EntityFrameworkCore;
using CategoriesService.Repositories.Data;
using Testcontainers.PostgreSql;
using CategoriesService.Models.DB;
using Microsoft.Extensions.Configuration;
using Xunit;
using CategoriesService.Data;

namespace CategoriesService.Tests.Integration.Postgres
{
    public class PostgreSqlContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; private set; } = null!;

        public string ConnectionString => Container.GetConnectionString();

        public async Task InitializeAsync()
        {

            PostgreSqlBuilder builder = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithPortBinding(5432)
                .WithCleanUp(true)
                .WithImage("postgres:17");

            Container = builder.Build();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            await Container.StartAsync();

            using var context = new CategoriesDbContext(
                new DbContextOptionsBuilder<CategoriesDbContext>()
                    .UseNpgsql(Container.GetConnectionString())
                    .Options, config);

            EnsureDbCreatedAndMigrated(context);

        }

        public void EnsureDbCreatedAndMigrated(CategoriesDbContext dbContext)
        {
            dbContext.Database.Migrate();
            SeedData.InitializeTest(dbContext);
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
        }
    }
}
