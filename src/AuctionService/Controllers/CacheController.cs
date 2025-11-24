using AuctionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;

    public CacheController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Check if a cache key exists
    /// </summary>
    [HttpGet("exists/{key}")]
    public async Task<ActionResult<bool>> CheckCacheExists(string key)
    {
        var exists = await _cacheService.ExistsAsync(key);
        return Ok(new { key, exists });
    }

    /// <summary>
    /// Remove a specific cache entry
    /// </summary>
    [HttpDelete("{key}")]
    public async Task<ActionResult> RemoveCache(string key)
    {
        await _cacheService.RemoveAsync(key);
        return Ok(new { message = $"Cache key '{key}' removed successfully" });
    }

    /// <summary>
    /// Remove all auction-related cache entries
    /// </summary>
    [HttpDelete("clear/auctions")]
    public async Task<ActionResult> ClearAuctionCache()
    {
        await _cacheService.RemoveAsync("auctions:all");
        await _cacheService.RemoveByPatternAsync("auction:*");
        return Ok(new { message = "All auction cache entries cleared" });
    }

    /// <summary>
    /// Get cache statistics (for monitoring)
    /// </summary>
    [HttpGet("stats")]
    public ActionResult GetCacheStats()
    {
        return Ok(new
        {
            message = "Cache is operational",
            cacheKeys = new[]
            {
                "auctions:all - List of all auctions",
                "auction:{id} - Individual auction by ID"
            },
            tips = new[]
            {
                "Use GET /api/cache/exists/{key} to check if a key exists",
                "Use DELETE /api/cache/{key} to remove a specific key",
                "Use DELETE /api/cache/clear/auctions to clear all auction cache"
            }
        });
    }
}
