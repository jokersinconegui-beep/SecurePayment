// src/WebApi/Middlewares/ErrorLoggingMiddleware.cs
using Serilog;

namespace WebApi.Middlewares;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;
    
    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred processing {Method} {Path}", 
                context.Request.Method, 
                context.Request.Path);
            throw;
        }
    }
}