using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Settings;

namespace Template.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly CacheOptions _cacheOptions;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IOptions<CacheOptions> cacheOptions)
        {
            _database = redis.GetDatabase();
            _logger = logger;
            _cacheOptions = cacheOptions.Value;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (!value.HasValue)
                {
                    if (_cacheOptions.EnableLogging)
                    {
                        _logger.LogDebug("Cache miss for key: {Key}", key);
                    }
                    return default;
                }

                if (_cacheOptions.EnableLogging)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                }

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            try
            {
                var expiry = expirationTime ?? TimeSpan.FromSeconds(_cacheOptions.DefaultDurationSeconds);
                var serializedValue = JsonSerializer.Serialize(value);

                await _database.StringSetAsync(key, serializedValue, expiry);

                if (_cacheOptions.EnableLogging)
                {
                    _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", key, expiry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                if (_cacheOptions.EnableLogging)
                {
                    _logger.LogDebug("Cache removed for key: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

    }
}


