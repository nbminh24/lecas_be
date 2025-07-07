using be_lecas.Models;

namespace be_lecas.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(string id);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<string> GenerateOrderNumberAsync();
    }
}

