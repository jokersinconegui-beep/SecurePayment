// tests/IntegrationTests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover todas las configuraciones existentes de DbContext
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(ApplicationDbContext)).ToList();
            
            foreach (var descriptor in descriptors)
                services.Remove(descriptor);
            
            // Agregar SQLite en memoria compartida
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connection);
                options.EnableSensitiveDataLogging();
            });
            
            // Construir el service provider y crear las tablas
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}