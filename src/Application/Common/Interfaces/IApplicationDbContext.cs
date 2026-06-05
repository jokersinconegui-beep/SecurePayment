// src/Application/Common/Interfaces/IApplicationDbContext.cs
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Usar IQueryable en lugar de DbSet (IQueryable está en System.Linq)
    IQueryable<Transaction> Transactions { get; }
    IQueryable<Merchant> Merchants { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}