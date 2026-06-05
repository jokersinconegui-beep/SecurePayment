// src/Infrastructure/Repositories/PaymentRepository.cs
using Application.Common.Interfaces;
using Application.DTOs.Transactions;
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
        return await _context.Transactions
            .Where(t => t.MerchantId == merchantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    // ✅ Nuevo método para transacciones paginadas y filtradas
    public async Task<TransactionListResponse> GetTransactionsByMerchantAsync(
        string merchantId,
        int page,
        int pageSize,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        // Validar página y tamaño
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Construir la consulta base
        var query = _context.Transactions
            .Where(t => t.MerchantId == merchantId);

        // Aplicar filtros
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.Date.AddDays(1);
            query = query.Where(t => t.CreatedAt < endDate);
        }

        // Obtener total de registros (para paginación)
        var totalCount = await query.CountAsync(cancellationToken);

        // Obtener transacciones paginadas
        var transactions = await query
    .OrderByDescending(t => t.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(t => new TransactionDto
    {
        TransactionId = t.Id,
        MaskedCardNumber = t.CardNumber.Masked,
        Amount = t.Amount.Amount,
        Currency = t.Amount.Currency,
        Status = t.Status.ToString(),
        CreatedAt = t.CreatedAt,
        // ✅ Usar Substring en lugar del operador .. (rango)
        ApprovalCode = t.Status == TransactionStatus.Approved
            ? "APR" + t.Id.ToString().Substring(0, 6)
            : null
    })
    .ToListAsync(cancellationToken);

        return new TransactionListResponse
        {
            Transactions = transactions,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}