using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using TransactionsService.Data;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Tests.Integration.Postgres
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

            using var context = new TransactionsDbContext(
                new DbContextOptionsBuilder<TransactionsDbContext>()
                    .UseNpgsql(Container.GetConnectionString())
                    .Options, config);

            EnsureDbCreatedAndMigrated(context);

        }

        public void EnsureDbCreatedAndMigrated(TransactionsDbContext dbContext)
        {
            dbContext.Database.Migrate();
            SeedData.InitializeTest(dbContext);
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
            await Task.Delay(TimeSpan.FromSeconds(15));
        }
    }
}
