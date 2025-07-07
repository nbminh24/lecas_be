using be_lecas.Models;

namespace be_lecas.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(string id);
        Task<List<Category>> GetAllAsync();
        Task<List<Category>> GetRootCategoriesAsync();
        Task<List<Category>> GetChildrenAsync(string parentId);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<List<Category>> GetActiveCategoriesAsync();
    }
} 