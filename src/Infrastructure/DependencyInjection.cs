// src/Infrastructure/DependencyInjection.cs (actualizado)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure;

public static class DependencyInjection
{
   // src/Infrastructure/DependencyInjection.cs (actualizar la parte de repositorios)

public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // Base de datos
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    
    // Redis Cache
    var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SecurePayment_";
    });
    
    services.AddScoped<ICacheService, RedisCacheService>();
    
    // Repositorio con decorador de caché (decorator pattern)
    services.AddScoped<PaymentRepository>();
    services.AddScoped<IPaymentRepository>(provider =>
    {
        var decorated = provider.GetRequiredService<PaymentRepository>();
        var cache = provider.GetRequiredService<ICacheService>();
        return new CachedPaymentRepository(decorated, cache);
    });
    
    return services;
}
}