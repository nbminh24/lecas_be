using MongoDB.Driver;
using be_lecas.Models;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            var databaseName = configuration.GetSection("ConnectionStrings:DatabaseName").Value ?? "lecas";
            var database = client.GetDatabase(databaseName);
            _users = database.GetCollection<User>("users");
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await _users.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _users.Find(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _users.Find(u => u.Id == id).AnyAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).AnyAsync();
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _users.Find(u => u.EmailVerificationToken == token).FirstOrDefaultAsync();
        }
    }
}

