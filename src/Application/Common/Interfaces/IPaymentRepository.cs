// src/Application/Common/Interfaces/IPaymentRepository.cs
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task SaveAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    
    // ✅ Agregar estos métodos
    Task<IEnumerable<Transaction>> GetByMerchantAsync(string merchantId, CancellationToken cancellationToken);
    Task<bool> ExistsByKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
}