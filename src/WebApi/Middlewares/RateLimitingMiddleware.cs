// src/WebApi/Middlewares/RateLimitingMiddleware.cs
using System.Collections.Concurrent;

namespace WebApi.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, int> _requestCounts = new();
    private static readonly ConcurrentDictionary<string, DateTime> _resetTimes = new();
    
    public RateLimitingMiddleware(RequestDelegate next)
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
        
        // Agregar headers para debugging
        context.Response.Headers["X-RateLimit-IP"] = clientIp;
        context.Response.Headers["X-RateLimit-Count"] = count.ToString();
        context.Response.Headers["X-RateLimit-Limit"] = "100";
        
        // Verificar límite (100 por minuto)
        if (count > 100)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["X-RateLimit-Reset"] = _resetTimes[clientIp].ToString("o");
            await context.Response.WriteAsync("{\"error\":\"Too Many Requests\",\"retryAfter\":60}");
            return;
        }
        
        await _next(context);
    }
}