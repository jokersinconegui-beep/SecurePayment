// src/WebApi/HealthChecks/RedisHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace WebApi.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    
    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis is responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is not responding", ex);
        }
    }
}