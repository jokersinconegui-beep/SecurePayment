// src/Infrastructure/Repositories/PaymentRepository.cs
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    
    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task SaveAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByMerchantAsync(string merchantId, CancellationToken cancellationToken)
    {
        // Nota: MerchantId aún no está en Transaction. Lo agregaremos después.
        return await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Approved)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<bool> ExistsByKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        // Nota: IdempotencyKey aún no está en Transaction. Lo agregaremos después.
        return await _context.Transactions
            .AnyAsync(t => t.Id.ToString() == idempotencyKey, cancellationToken);
    }
}