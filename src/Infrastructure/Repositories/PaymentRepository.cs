// src/Infrastructure/Repositories/PaymentRepository.cs
using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    // Simulación de base de datos en memoria
    private static readonly Dictionary<Guid, Transaction> _transactions = new();
    
    public Task SaveAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        _transactions[transaction.Id] = transaction;
        return Task.CompletedTask;
    }
    
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _transactions.TryGetValue(id, out var transaction);
        return Task.FromResult(transaction);
    }
}