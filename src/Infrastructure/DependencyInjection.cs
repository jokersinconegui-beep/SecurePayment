// src/Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Prometheus;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Base de datos
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        
        // Repositorios
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
    
    // ✅ Renombrado a AddPrometheusMetrics para evitar conflicto
    public static IServiceCollection AddPrometheusMetrics(this IServiceCollection services)
    {
        // Registrar MetricsService
        services.AddSingleton<IMetricsService, MetricsService>();
        
        return services;
    }
    
    // ✅ Renombrado a UsePrometheusMetrics para evitar conflicto
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app)
    {
        // Configurar Prometheus
        app.UseHttpMetrics();   // Métricas automáticas de requests HTTP
        app.UseMetricServer();  // Endpoint /metrics
        
        return app;
    }
}