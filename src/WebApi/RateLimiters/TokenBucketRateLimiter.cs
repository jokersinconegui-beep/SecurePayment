// src/WebApi/RateLimiters/TokenBucketRateLimiter.cs
namespace WebApi.RateLimiters;

public class TokenBucketRateLimiter
{
    private readonly double _tokensPerSecond;
    private readonly double _maxTokens;
    private double _currentTokens;
    private DateTime _lastRefill;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public TokenBucketRateLimiter(double tokensPerSecond, double maxTokens)
    {
        _tokensPerSecond = tokensPerSecond;
        _maxTokens = maxTokens;
        _currentTokens = maxTokens;
        _lastRefill = DateTime.UtcNow;
    }
    
    public async Task<bool> TryConsumeAsync(int tokens = 1)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Primero recargar tokens basado en el tiempo transcurrido
            RefillTokens();
            
            // Verificar si hay suficientes tokens
            if (_currentTokens >= tokens)
            {
                _currentTokens -= tokens;
                return true;
            }
            
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastRefill).TotalSeconds;
        
        if (timePassed > 0)
        {
            var tokensToAdd = timePassed * _tokensPerSecond;
            _currentTokens = Math.Min(_maxTokens, _currentTokens + tokensToAdd);
            _lastRefill = now;
        }
    }
    
    public async Task<double> GetTimeUntilNextTokenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            RefillTokens();
            
            if (_currentTokens >= 1)
                return 0;
            
            return (1 - _currentTokens) / _tokensPerSecond;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<int> GetAvailableTokensAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            RefillTokens();
            return (int)Math.Floor(_currentTokens);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task ResetAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _currentTokens = _maxTokens;
            _lastRefill = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}