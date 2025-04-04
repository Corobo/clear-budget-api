using CategoriesService.Models.DB;
using CategoriesService.Repositories.Data;

namespace CategoriesService.Data;

public static class SeedData
{
    public static void Initialize(CategoriesDbContext context)
    {
        if (context.Categories.Any())
            return; // DB already seeded

        var defaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var categories = new List<Category>
        {
            // Admin/global categories
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Transport",
                Color = "#FF5733",
                UserId = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Groceries",
                Color = "#33C3FF",
                UserId = null
            },

            // User-specific categories
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gym",
                Color = "#9C27B0",
                UserId = defaultUserId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Color = "#4CAF50",
                UserId = defaultUserId
            }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();
    }

    public static void InitializeTest(CategoriesDbContext context)
    {
        if (context.Categories.Any()) return;

        var defaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        context.Categories.AddRange(new[]
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Seed Category",
                Color = "#123456",
                UserId = defaultUserId
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Admin Category",
                Color = "#654321",
                UserId = null
            }
        });

        context.SaveChanges();
    }
}
