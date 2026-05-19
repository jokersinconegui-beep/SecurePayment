// src/Application/Common/Interfaces/IPaymentRepository.cs
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task SaveAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}