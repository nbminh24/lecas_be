using be_lecas.Models;
using be_lecas.DTOs;

namespace be_lecas.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(string id);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByCategoryAsync(string categoryId);
        Task<List<Product>> SearchAsync(string searchTerm);
        Task<List<Product>> GetFilteredAsync(ProductFilterRequest filter);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<List<Product>> GetRelatedProductsAsync(string productId, int limit = 10);
        Task<List<Product>> GetTopSellingAsync(int limit = 10);
        Task<List<Product>> GetFlashSaleAsync();
        Task<int> GetTotalCountAsync(ProductFilterRequest? filter = null);
    }
}

