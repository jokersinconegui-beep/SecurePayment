// src/Infrastructure/Services/RedisCacheService.cs
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;
    
    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(data))
            return default;
        
        return JsonSerializer.Deserialize<T>(data);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        else
        {
            options.AbsoluteExpirationRelativeToNow = _defaultOptions.AbsoluteExpirationRelativeToNow;
            options.SlidingExpiration = _defaultOptions.SlidingExpiration;
        }
        
        var jsonData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonData, options, cancellationToken);
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(data);
    }
    
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;
        
        var result = await factory();
        await SetAsync(key, result, expiration, cancellationToken);
        return result;
    }
}