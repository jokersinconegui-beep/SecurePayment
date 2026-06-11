// src/WebApi/HealthChecks/MemoryHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApi.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _minFreeMemoryMB;
    
    // ✅ Agregar constructor sin parámetros con valor por defecto
    public MemoryHealthCheck() : this(512) { }
    
    public MemoryHealthCheck(long minFreeMemoryMB)
    {
        _minFreeMemoryMB = minFreeMemoryMB;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        var availableMemory = memoryInfo.TotalAvailableMemoryBytes / (1024 * 1024);
        
        if (availableMemory < _minFreeMemoryMB)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Low memory available: {availableMemory}MB free (minimum required: {_minFreeMemoryMB}MB)"));
        }
        
        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory available: {availableMemory}MB"));
    }
}