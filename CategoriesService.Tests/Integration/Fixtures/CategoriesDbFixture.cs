using Shared.Testing.Containers;
using CategoriesService.Data;
using CategoriesService.Repositories.Data;

namespace TransactionsService.Tests.Integration.Fixtures
{
    public class CategoriesDbFixture : PostgreSqlContainerFixture<CategoriesDbContext>
    {
        public CategoriesDbFixture() : base(
            (options, config) => new CategoriesDbContext(options, config),
            dbContext => SeedData.InitializeTest(dbContext))
        {
        }
    }
}
