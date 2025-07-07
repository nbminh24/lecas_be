using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace be_lecas.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache distributedCache, IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                // Thử Redis trước
                var data = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(data))
                {
                    return JsonSerializer.Deserialize<T>(data);
                }

                // Fallback to memory cache
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    return cachedValue;
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis cache failed, using memory cache for key: {Key}", key);
                
                // Fallback to memory cache
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    return cachedValue;
                }

                return default;
            }
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan expiry)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                
                // Set to Redis
                await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                });

                // Also set to memory cache as backup
                _memoryCache.Set(key, data, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis cache failed, using memory cache for key: {Key}", key);
                
                // Fallback to memory cache only
                _memoryCache.Set(key, data, expiry);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis remove failed for key: {Key}", key);
            }
            
            _memoryCache.Remove(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(data))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis exists check failed for key: {Key}", key);
            }

            return _memoryCache.TryGetValue(key, out _);
        }
    }
} 