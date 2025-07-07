using System.Text.Json;

namespace be_lecas.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T data, TimeSpan expiry);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
} 