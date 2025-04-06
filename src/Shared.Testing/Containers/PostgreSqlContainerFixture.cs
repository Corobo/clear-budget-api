using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Shared.Testing.Containers
{
    public class PostgreSqlContainerFixture<TContext> : IAsyncLifetime where TContext : DbContext
    {
        public PostgreSqlContainer Container { get; private set; } = null!;
        public string ConnectionString => Container.GetConnectionString();

        private readonly Func<DbContextOptions<TContext>, IConfiguration, TContext> _contextFactory;
        private readonly Action<TContext>? _initialize;

        public PostgreSqlContainerFixture(
            Func<DbContextOptions<TContext>, IConfiguration, TContext> contextFactory,
            Action<TContext>? initialize = null)
        {
            _contextFactory = contextFactory;
            _initialize = initialize;
        }

        public async Task InitializeAsync()
        {
            Container = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithPortBinding(5432, 5432)
                .WithCleanUp(true)
                .WithImage("postgres:17")
                .Build();

            await Container.StartAsync();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(Container.GetConnectionString())
                .Options;

            using var context = _contextFactory(options, config);

            context.Database.Migrate();

            _initialize?.Invoke(context);
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
