// src/Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Reflection;

namespace Infrastructure.Services.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ✅ Usar el interceptor inyectado
        if (_auditInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }
        base.OnConfiguring(optionsBuilder);
    }
    
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}