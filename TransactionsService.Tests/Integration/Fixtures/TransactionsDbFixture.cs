using Shared.Testing.Containers;
using TransactionsService.Data;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Tests.Integration.Fixtures
{
    public class TransactionsDbFixture : PostgreSqlContainerFixture<TransactionsDbContext>
    {
        public TransactionsDbFixture() : base(
            (options, config) => new TransactionsDbContext(options, config),
            dbContext => SeedData.InitializeTest(dbContext))
        {
        }
    }
}
