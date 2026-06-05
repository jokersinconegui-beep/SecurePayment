// src/Application/Common/Interfaces/IPaymentRepository.cs
using Domain.Entities;
using Application.DTOs.Transactions;

namespace Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task SaveAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Transaction>> GetByMerchantAsync(string merchantId, CancellationToken cancellationToken);
    Task<bool> ExistsByKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    
    // ✅ Agregar este método
    Task<TransactionListResponse> GetTransactionsByMerchantAsync(
        string merchantId,
        int page,
        int pageSize,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken);
}