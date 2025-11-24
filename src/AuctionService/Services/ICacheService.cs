namespace AuctionService.Services;

public interface ICacheService
{
    /// <summary>
    /// Get a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Set a value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Remove a value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Remove multiple values from cache by pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
}
