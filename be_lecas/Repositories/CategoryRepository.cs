using MongoDB.Driver;
using be_lecas.Models;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("lecas");
            _categories = database.GetCollection<Category>("categories");
        }

        public async Task<Category?> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categories.Find(_ => true).ToListAsync();
        }

        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            return await _categories.Find(c => c.ParentId == null && c.IsActive).ToListAsync();
        }

        public async Task<List<Category>> GetChildrenAsync(string parentId)
        {
            return await _categories.Find(c => c.ParentId == parentId && c.IsActive).ToListAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _categories.InsertOneAsync(category);
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
            return category;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _categories.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).AnyAsync();
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _categories.Find(c => c.IsActive).ToListAsync();
        }
    }
} 