using CategoriesService.Models.DTO;

namespace CategoriesService.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetMergedCategoriesAsync(Guid userId);
        Task<IEnumerable<CategoryDTO>> GetAdminCategoriesAsync();
        Task<IEnumerable<Guid>> GetAllCategories();
        Task<CategoryDTO> CreateUserCategoryAsync(Guid userId, CategoryCreateDTO dto);
        Task<CategoryDTO> CreateAdminCategoryAsync(CategoryCreateDTO dto);
        Task<bool> UpdateCategoryColorAsync(Guid id, Guid userId, CategoryUpdateDTO dto);
        Task<bool> DeleteUserCategoryAsync(Guid id, Guid userId);
        Task<bool> DeleteAdminCategoryAsync(Guid id);
    }

}
