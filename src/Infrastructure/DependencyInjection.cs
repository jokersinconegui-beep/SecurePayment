// src/Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar base de datos (SQLite para desarrollo)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        return services;
    }
}