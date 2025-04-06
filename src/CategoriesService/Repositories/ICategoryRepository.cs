using CategoriesService.Models.DB;

namespace CategoriesService.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetUserCategoriesAsync(Guid userId);
        Task<List<Category>> GetAdminCategoriesAsync();
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task AddAsync(Category category);
        Task DeleteAsync(Category category);
        Task SaveChangesAsync();
    }
}
