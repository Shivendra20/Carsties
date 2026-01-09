using System.Text.Json;
using StackExchange.Redis;

namespace AuctionService.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly string _instanceName;
    private readonly int _defaultExpirationMinutes;

    public RedisCacheService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _instanceName = configuration["Redis:InstanceName"] ?? "Carsties_";
        _defaultExpirationMinutes = int.Parse(
            configuration["Redis:DefaultExpirationMinutes"] ?? "30"
        );
    }

    public async Task<T?> GetAsync<T>(string key)
        where T : class
    {
        var prefixedKey = GetPrefixedKey(key);
        var value = await _db.StringGetAsync(prefixedKey);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        where T : class
    {
        var prefixedKey = GetPrefixedKey(key);
        var serializedValue = JsonSerializer.Serialize(value);
        var expirationTime = expiration ?? TimeSpan.FromMinutes(_defaultExpirationMinutes);

        await _db.StringSetAsync(prefixedKey, serializedValue, expirationTime);
    }

    public async Task RemoveAsync(string key)
    {
        var prefixedKey = GetPrefixedKey(key);
        await _db.KeyDeleteAsync(prefixedKey);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var prefixedPattern = GetPrefixedKey(pattern);
        var endpoints = _redis.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            var keys = server.Keys(pattern: prefixedPattern);

            foreach (var key in keys)
            {
                await _db.KeyDeleteAsync(key);
            }
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var prefixedKey = GetPrefixedKey(key);
        return await _db.KeyExistsAsync(prefixedKey);
    }

    private string GetPrefixedKey(string key)
    {
        return $"{_instanceName}{key}";
    }
}
