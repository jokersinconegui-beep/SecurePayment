// src/Infrastructure/Persistence/Interceptors/AuditInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;  // ✅ Agregar este using


public class AuditInterceptor(ILogger<AuditInterceptor> logger) : SaveChangesInterceptor
{
    private readonly ILogger<AuditInterceptor> _logger = logger;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var entries = eventData.Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
            foreach (var entry in entries)
            {
                _logger.LogDebug("Entity {EntityType} {State} - {Keys}", 
                    entry.Entity.GetType().Name, 
                    entry.State,
                    string.Join(",", entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue)));
            }
        }
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}