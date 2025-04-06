using TransactionsService.Models.DB;
using TransactionsService.Models.Enums;
using TransactionsService.Repositories.Data;

namespace TransactionsService.Data
{
    public static class SeedData
    {
        public static void Initialize(TransactionsDbContext context)
        {
            if (context.Transactions.Any())
                return; // DB already seeded

            var defaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var defaultCategoryId = Guid.NewGuid(); // podrías relacionar con categorías reales si las tienes

            var transactions = new List<Transaction>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = defaultUserId,
                    CategoryId = defaultCategoryId,
                    Amount = 75.50m,
                    Description = "Monthly gym membership",
                    Type = TransactionType.Expense,
                    Date = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = defaultUserId,
                    CategoryId = defaultCategoryId,
                    Amount = 2000.00m,
                    Description = "Salary for March",
                    Type = TransactionType.Income,
                    Date = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }

        public static void InitializeTest(TransactionsDbContext context)
        {
            if (context.Transactions.Any())
                return;

            var defaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var defaultCategoryId = Guid.NewGuid();

            context.Transactions.AddRange(new[]
            {
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = defaultUserId,
                    CategoryId = defaultCategoryId,
                    Amount = 10.00m,
                    Description = "Test Transaction 1",
                    Type = TransactionType.Expense,
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = defaultUserId,
                    CategoryId = defaultCategoryId,
                    Amount = 100.00m,
                    Description = "Test Transaction 2",
                    Type = TransactionType.Income,
                    Date = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow
                }
            });

            context.SaveChanges();
        }
    }
}
