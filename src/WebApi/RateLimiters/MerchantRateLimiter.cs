// src/WebApi/RateLimiters/MerchantRateLimiter.cs
using System.Collections.Concurrent;

namespace WebApi.RateLimiters;

// src/WebApi/RateLimiters/MerchantRateLimiter.cs
using System.Collections.Concurrent;


// src/WebApi/RateLimiters/MerchantRateLimiter.cs
public class MerchantRateLimiter
{
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();

    public Task<bool> IsAllowedAsync(string merchantId, int rateLimit)
    {
        // ✅ Siempre obtener el counter actualizado (puede haber sido refrescado)
        var counter = _counters.GetOrAdd(merchantId, _ => new SlidingWindowCounter(rateLimit));

        // ✅ Si el límite cambió, recrear el counter
        if (counter.MaxRequests != rateLimit)
        {
            Console.WriteLine($"🔄 Límite cambiado para {merchantId}: {counter.MaxRequests} → {rateLimit}");
            counter = new SlidingWindowCounter(rateLimit);
            _counters.AddOrUpdate(merchantId, counter, (_, _) => counter);
        }

        var allowed = counter.TryConsume();
        Console.WriteLine($"📊 {merchantId} - Allowed: {allowed} - Current: {counter.GetCurrentCount()}/{rateLimit}");

        return Task.FromResult(allowed);
    }

    public Task<int> GetRemainingTokensAsync(string merchantId, int rateLimit)
    {
        var counter = _counters.GetOrAdd(merchantId, _ => new SlidingWindowCounter(rateLimit));

        if (counter.MaxRequests != rateLimit)
        {
            counter = new SlidingWindowCounter(rateLimit);
            _counters.AddOrUpdate(merchantId, counter, (_, _) => counter);
        }

        var remaining = rateLimit - counter.GetCurrentCount();
        return Task.FromResult(remaining);
    }

    public void RefreshLimiter(string merchantId)
    {
        // ✅ Eliminar el contador para que se cree uno nuevo en la próxima request
        if (_counters.TryRemove(merchantId, out var removed))
        {
            Console.WriteLine($"🔄 Rate limiter refrescado para merchant: {merchantId} (límite anterior: {removed.MaxRequests})");
        }
        else
        {
            Console.WriteLine($"ℹ️ No se encontró limiter activo para {merchantId}");
        }
    }
}

public class SlidingWindowCounter
{
    private readonly int _maxRequests;
    private int _currentCount;
    private DateTime _windowStart;
    private readonly object _lock = new();
    public int MaxRequests => _maxRequests;  // ✅ Propiedad pública para verificar límite

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