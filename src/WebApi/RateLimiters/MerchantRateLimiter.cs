// src/WebApi/RateLimiters/MerchantRateLimiter.cs
using System.Collections.Concurrent;

namespace WebApi.RateLimiters;

public class MerchantRateLimiter
{
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();
    
    public Task<bool> IsAllowedAsync(string merchantId, int rateLimit)
    {
        var counter = _counters.GetOrAdd(merchantId, _ => new SlidingWindowCounter(rateLimit));
        var allowed = counter.TryConsume();
        
        Console.WriteLine($"📊 {merchantId} - Allowed: {allowed} - Current: {counter.GetCurrentCount()}/{rateLimit}");
        
        return Task.FromResult(allowed);
    }
    
    public Task<int> GetRemainingTokensAsync(string merchantId, int rateLimit)
    {
        var counter = _counters.GetOrAdd(merchantId, _ => new SlidingWindowCounter(rateLimit));
        var remaining = rateLimit - counter.GetCurrentCount();
        return Task.FromResult(remaining);
    }
}

public class SlidingWindowCounter
{
    private readonly int _maxRequests;
    private int _currentCount;
    private DateTime _windowStart;
    private readonly object _lock = new();
    
    public SlidingWindowCounter(int maxRequests)
    {
        _maxRequests = maxRequests;
        _currentCount = 0;
        _windowStart = DateTime.UtcNow;
    }
    
    public bool TryConsume()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            // Si pasó más de 1 minuto, reiniciar la ventana
            if ((now - _windowStart).TotalSeconds >= 60)
            {
                _currentCount = 0;
                _windowStart = now;
                Console.WriteLine($"   🔄 Window reset for new minute");
            }
            
            // Verificar si podemos consumir
            if (_currentCount < _maxRequests)
            {
                _currentCount++;
                Console.WriteLine($"   ✅ Consumed. Count: {_currentCount}/{_maxRequests}");
                return true;
            }
            
            Console.WriteLine($"   ❌ Rejected. Count: {_currentCount}/{_maxRequests}");
            return false;
        }
    }
    
    public int GetCurrentCount()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            if ((now - _windowStart).TotalSeconds >= 60)
            {
                return 0;
            }
            return _currentCount;
        }
    }
}