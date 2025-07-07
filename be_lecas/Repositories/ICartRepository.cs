using be_lecas.Models;

namespace be_lecas.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(string userId);
        Task<Cart> CreateAsync(Cart cart);
        Task<Cart> UpdateAsync(Cart cart);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}

