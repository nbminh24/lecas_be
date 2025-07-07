using be_lecas.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace be_lecas.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly IMongoCollection<Promotion> _promotions;

        public PromotionRepository(IMongoDatabase database)
        {
            _promotions = database.GetCollection<Promotion>("Promotions");
        }

        public async Task<Promotion?> GetByIdAsync(string id)
        {
            return await _promotions.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Promotion>> GetAllAsync()
        {
            return await _promotions.Find(_ => true).ToListAsync();
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            var now = System.DateTime.UtcNow;
            return await _promotions.Find(x => x.IsActive && x.StartDate <= now && x.EndDate >= now).ToListAsync();
        }

        public async Task<Promotion> CreateAsync(Promotion promotion)
        {
            await _promotions.InsertOneAsync(promotion);
            return promotion;
        }

        public async Task<Promotion?> UpdateAsync(string id, Promotion promotion)
        {
            var result = await _promotions.ReplaceOneAsync(x => x.Id == id, promotion);
            if (result.IsAcknowledged && result.ModifiedCount > 0)
                return promotion;
            return null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _promotions.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
