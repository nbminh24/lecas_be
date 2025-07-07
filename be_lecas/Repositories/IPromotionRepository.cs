using be_lecas.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace be_lecas.Repositories
{
    public interface IPromotionRepository
    {
        Task<Promotion?> GetByIdAsync(string id);
        Task<List<Promotion>> GetAllAsync();
        Task<List<Promotion>> GetActivePromotionsAsync();
        Task<Promotion> CreateAsync(Promotion promotion);
        Task<Promotion?> UpdateAsync(string id, Promotion promotion);
        Task<bool> DeleteAsync(string id);
    }
}
