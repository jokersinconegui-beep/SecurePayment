// src/WebApi/Middlewares/SimpleRateLimitingMiddleware.cs
using System.Collections.Concurrent;

namespace WebApi.Middlewares;

public class SimpleRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, int> _requestCounts = new();
    private static readonly ConcurrentDictionary<string, DateTime> _resetTimes = new();
    
    public SimpleRateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;
        
        // Resetear contador cada minuto
        if (!_resetTimes.TryGetValue(clientIp, out var resetTime) || now > resetTime)
        {
            _requestCounts[clientIp] = 0;
            _resetTimes[clientIp] = now.AddMinutes(1);
        }
        
        // Incrementar contador
        var count = _requestCounts.AddOrUpdate(clientIp, 1, (_, c) => c + 1);
        
        // Verificar límite (100 por minuto)
        if (count > 100)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Too Many Requests");
            return;
        }
        
        await _next(context);
    }
}