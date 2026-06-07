// src/WebApi/Middlewares/MerchantRateLimitingMiddleware.cs
using System.Security.Claims;
using System.Collections.Concurrent;
using WebApi.RateLimiters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Middlewares;

public class MerchantRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MerchantRateLimitingMiddleware> _logger;
    private static readonly MerchantRateLimiter _rateLimiter = new();
    
    public MerchantRateLimitingMiddleware(RequestDelegate next, ILogger<MerchantRateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // ✅ CORREGIDO: Extraer MerchantId correctamente
        var merchantId = context.User.Claims.FirstOrDefault(c => c.Type == "MerchantId")?.Value;
        
        if (string.IsNullOrEmpty(merchantId))
        {
            merchantId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        
        Console.WriteLine($"🔍 MerchantId encontrado: '{merchantId}'");
        
        if (string.IsNullOrEmpty(merchantId))
        {
            Console.WriteLine("⚠️ No MerchantId found in token - skipping rate limiting");
            await _next(context);
            return;
        }
        
        // Obtener el plan del merchant desde la base de datos
        var merchant = await dbContext.Merchants
            .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
        
        if (merchant == null)
        {
            Console.WriteLine($"⚠️ Merchant {merchantId} not found in DB");
            await _next(context);
            return;
        }
        
        var rateLimit = merchant.GetRateLimit();
        Console.WriteLine($"📊 Merchant: {merchantId}, Plan: {merchant.Plan}, Limit: {rateLimit}");
        
        // Verificar rate limit
        if (!await _rateLimiter.IsAllowedAsync(merchantId, rateLimit))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "60";
            context.Response.Headers["X-RateLimit-Limit"] = rateLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            
            _logger.LogWarning("Rate limit exceeded for merchant {MerchantId} (Plan: {Plan})", 
                merchantId, merchant.Plan);
            return;
        }
        
        // Agregar headers informativos
        var remaining = await _rateLimiter.GetRemainingTokensAsync(merchantId, rateLimit);
        context.Response.Headers["X-RateLimit-Limit"] = rateLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Plan"] = merchant.Plan.ToString();
        
        await _next(context);
    }
}