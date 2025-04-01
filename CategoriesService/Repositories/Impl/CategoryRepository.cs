using CategoriesService.Models.DB;
using CategoriesService.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace CategoriesService.Repositories.Impl
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoriesDbContext _db;

        public CategoryRepository(CategoriesDbContext db)
        {
            _db = db;
        }

        public Task<List<Category>> GetUserCategoriesAsync(Guid userId)
        {
            return _db.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public Task<List<Category>> GetAdminCategoriesAsync()
        {
            return _db.Categories
                .Where(c => c.UserId == null)
                .ToListAsync();
        }

        public Task<Category?> GetByIdAsync(Guid id)
        {
            return _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            await _db.Categories.AddAsync(category);
        }

        public Task DeleteAsync(Category category)
        {
            _db.Categories.Remove(category);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
