// src/WebApi/HealthChecks/RedisHealthCheck.cs
using StackExchange.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApi.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var ping = await db.PingAsync();
            
            return HealthCheckResult.Healthy($"Redis responded in {ping.TotalMilliseconds:F2}ms");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis health check failed");
            return HealthCheckResult.Degraded("Redis is not responding", ex);
        }
    }
}