using MongoDB.Driver;
using be_lecas.Models;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("lecas");
            _orders = database.GetCollection<Order>("orders");
        }

        public async Task<Order?> GetByIdAsync(string id)
        {
            return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            return await _orders.Find(o => o.UserId == userId)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.OrderNumber = await GenerateOrderNumberAsync();
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);
            return order;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _orders.DeleteOneAsync(o => o.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _orders.Find(o => o.Id == id).AnyAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow;
            var datePrefix = today.ToString("yyyyMMdd");
            
            // Get count of orders today
            var startOfDay = today.Date;
            var endOfDay = startOfDay.AddDays(1);
            
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Gte(o => o.CreatedAt, startOfDay),
                Builders<Order>.Filter.Lt(o => o.CreatedAt, endOfDay)
            );
            
            var count = await _orders.CountDocumentsAsync(filter);
            var orderNumber = $"{datePrefix}-{(count + 1):D4}";
            
            return orderNumber;
        }
    }
}

