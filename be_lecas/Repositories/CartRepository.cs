using MongoDB.Driver;
using be_lecas.Models;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IMongoCollection<Cart> _carts;

        public CartRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            var databaseName = configuration.GetSection("ConnectionStrings:DatabaseName").Value ?? "lecas";
            var database = client.GetDatabase(databaseName);
            _carts = database.GetCollection<Cart>("carts");
        }

        public async Task<Cart?> GetByUserIdAsync(string userId)
        {
            return await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<Cart> CreateAsync(Cart cart)
        {
            await _carts.InsertOneAsync(cart);
            return cart;
        }

        public async Task<Cart> UpdateAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return cart;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _carts.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _carts.Find(c => c.Id == id).AnyAsync();
        }
    }
}

