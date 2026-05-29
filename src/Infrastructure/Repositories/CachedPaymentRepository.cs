// src/Infrastructure/Repositories/CachedPaymentRepository.cs
using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class CachedPaymentRepository : IPaymentRepository
{
    private readonly IPaymentRepository _decorated;
    private readonly ICacheService _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    
    public CachedPaymentRepository(IPaymentRepository decorated, ICacheService cache)
    {
        _decorated = decorated;
        _cache = cache;
    }
    
    public async Task SaveAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _decorated.SaveAsync(transaction, cancellationToken);
        
        // Invalidar caché al guardar
        var cacheKey = $"transaction_{transaction.Id}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        
        // También invalidar caché de listas (simplificado)
        await _cache.RemoveAsync("transactions_recent", cancellationToken);
    }
    
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"transaction_{id}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _decorated.GetByIdAsync(id, cancellationToken);
        }, _cacheExpiration, cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByMerchantAsync(string merchantId, CancellationToken cancellationToken)
    {
        var cacheKey = $"transactions_merchant_{merchantId}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _decorated.GetByMerchantAsync(merchantId, cancellationToken);
        }, _cacheExpiration, cancellationToken);
    }
    
    public async Task<bool> ExistsByKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var cacheKey = $"idempotency_{idempotencyKey}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _decorated.ExistsByKeyAsync(idempotencyKey, cancellationToken);
        }, TimeSpan.FromHours(24), cancellationToken);
    }
}