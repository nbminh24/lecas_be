using be_lecas.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace be_lecas.Services
{
    public interface IPromotionService
    {
        Task<PromotionDto?> GetByIdAsync(string id);
        Task<List<PromotionDto>> GetAllAsync();
        Task<List<PromotionDto>> GetActivePromotionsAsync();
        Task<PromotionDto> CreateAsync(CreatePromotionRequest request);
        Task<PromotionDto?> UpdateAsync(string id, UpdatePromotionRequest request);
        Task<bool> DeleteAsync(string id);
    }
}
