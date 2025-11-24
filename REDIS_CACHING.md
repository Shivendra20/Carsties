# Redis Caching Implementation

This document describes the Redis caching implementation in the AuctionService backend.

## Overview

Redis caching has been integrated to improve API performance by reducing database queries for frequently accessed data. The implementation includes:

- **Redis Docker container** for local development
- **Cache service abstraction** for easy testing and maintenance
- **Automatic cache invalidation** on data updates
- **Configurable expiration times**

## Architecture

### Components

1. **ICacheService** - Interface defining caching operations
2. **RedisCacheService** - Redis implementation of the cache service
3. **AuctionsController** - Updated to use caching for auction data

### Cache Keys

The following cache keys are used:

- `auctions:all` - List of all auctions
- `auction:{id}` - Individual auction by ID

All keys are prefixed with `Carsties_` (configurable in appsettings.json)

## Configuration

### appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "Carsties_",
    "DefaultExpirationMinutes": 30
  }
}
```

- **ConnectionString**: Redis server connection string
- **InstanceName**: Prefix for all cache keys (useful for multi-tenancy)
- **DefaultExpirationMinutes**: Default cache expiration time

## Setup Instructions

### 1. Start Redis Container

Make sure Docker Desktop is running, then execute:

```bash
docker-compose up -d redis
```

This will start a Redis container on port 6379.

### 2. Verify Redis is Running

```bash
docker ps
```

You should see the Redis container running.

### 3. Restore NuGet Packages

```bash
cd src/AuctionService
dotnet restore
```

### 4. Run the Application

```bash
dotnet run
```

## How It Works

### Read Operations (GET)

1. **Check Cache**: First, the controller checks if data exists in Redis
2. **Cache Hit**: If found, return cached data immediately
3. **Cache Miss**: If not found, query the database
4. **Store in Cache**: Save the database result in Redis with expiration time
5. **Return Data**: Return the data to the client

### Write Operations (POST/PUT/DELETE)

1. **Perform Database Operation**: Update or delete data in the database
2. **Invalidate Cache**: Remove affected cache entries
3. **Return Response**: Return success/failure to the client

### Cache Invalidation Strategy

- **Update Auction**: Invalidates both `auction:{id}` and `auctions:all`
- **Delete Auction**: Invalidates both `auction:{id}` and `auctions:all`

This ensures data consistency between cache and database.

## Benefits

1. **Performance**: Reduced database load and faster response times
2. **Scalability**: Can handle more concurrent requests
3. **Cost Efficiency**: Lower database resource usage
4. **Flexibility**: Easy to adjust cache expiration times

## Cache Service Methods

### GetAsync<T>(string key)
Retrieves a cached value by key. Returns null if not found.

### SetAsync<T>(string key, T value, TimeSpan? expiration)
Stores a value in cache with optional expiration time.

### RemoveAsync(string key)
Removes a single cache entry.

### RemoveByPatternAsync(string pattern)
Removes multiple cache entries matching a pattern.

### ExistsAsync(string key)
Checks if a key exists in cache.

## Testing Cache

### Using Redis CLI

Connect to Redis container:
```bash
docker exec -it <container-name> redis-cli
```

View all keys:
```bash
KEYS *
```

Get a specific value:
```bash
GET Carsties_auctions:all
```

Check TTL (time to live):
```bash
TTL Carsties_auctions:all
```

Clear all cache:
```bash
FLUSHALL
```

### Using API

1. **First Request** (Cache Miss):
   ```
   GET /api/Auctions
   ```
   This will query the database and store in cache.

2. **Second Request** (Cache Hit):
   ```
   GET /api/Auctions
   ```
   This will return from cache (much faster).

3. **Update Data**:
   ```
   POST /api/Auctions/update/{id}
   ```
   This will invalidate the cache.

4. **Next Request** (Cache Miss Again):
   ```
   GET /api/Auctions
   ```
   This will query the database again and refresh the cache.

## Production Considerations

### For Production Deployment:

1. **Use Redis Cloud Service**: Instead of local Docker, use Azure Redis Cache, AWS ElastiCache, or Redis Cloud
2. **Update Connection String**: Change `appsettings.Production.json`:
   ```json
   {
     "Redis": {
       "ConnectionString": "your-production-redis-url:6379,password=your-password,ssl=True"
     }
   }
   ```

3. **Enable SSL**: For production, always use SSL/TLS
4. **Set Appropriate Expiration**: Adjust based on data update frequency
5. **Monitor Cache Hit Rate**: Use Redis monitoring tools
6. **Implement Circuit Breaker**: Handle Redis failures gracefully

## Troubleshooting

### Redis Connection Failed

**Error**: `It was not possible to connect to the redis server(s)`

**Solutions**:
- Ensure Docker Desktop is running
- Check if Redis container is running: `docker ps`
- Verify port 6379 is not in use by another service
- Check connection string in appsettings.json

### Cache Not Invalidating

**Solutions**:
- Check if cache invalidation code is being executed
- Verify the cache keys match exactly
- Use Redis CLI to manually check keys

### Performance Not Improving

**Solutions**:
- Verify cache is actually being hit (add logging)
- Check cache expiration time is appropriate
- Monitor Redis memory usage
- Ensure Redis is on the same network/region as your app

## Future Enhancements

1. **Distributed Caching**: For multi-instance deployments
2. **Cache Warming**: Pre-populate cache on startup
3. **Cache Aside Pattern**: More sophisticated caching strategies
4. **Monitoring**: Add cache hit/miss metrics
5. **Compression**: Compress large cached objects
6. **Pub/Sub**: Real-time cache invalidation across instances

## Dependencies

- **StackExchange.Redis** (v2.8.16): High-performance Redis client for .NET
- **System.Text.Json**: For JSON serialization of cached objects

## References

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis GitHub](https://github.com/StackExchange/StackExchange.Redis)
- [ASP.NET Core Distributed Caching](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
