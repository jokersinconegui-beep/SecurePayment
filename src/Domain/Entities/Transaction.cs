using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public CardNumber CardNumber { get; private set; }  // ← Nullable? No, se asigna en Create
    public Money Amount { get; private set; }           // ← Nullable? No, se asigna en Create
    public DateTime CreatedAt { get; private set; }
    public TransactionStatus Status { get; private set; }
    
    // Constructor privado para EF Core (requiere parámetros o usar = null!)
    private Transaction() { }  // EF Core lo usará
    
    // Constructor privado con parámetros
    private Transaction(CardNumber cardNumber, Money amount)
    {
        Id = Guid.NewGuid();
        CardNumber = cardNumber;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        Status = TransactionStatus.Pending;
    }
    
    public static Result<Transaction> Create(CardNumber card, Money amount, Cvv cvv)
    {
        if (!amount.IsPositive())
            return Result<Transaction>.Failure("Transaction amount must be positive");
        
        // Validar que el CVV sea compatible con la marca de la tarjeta
        var brandValidation = cvv.ValidateForBrand(card.GetBrand());
        if (brandValidation.IsFailure)
            return Result<Transaction>.Failure(brandValidation.Error);
        
        var transaction = new Transaction(card, amount);
        return Result<Transaction>.Success(transaction);
        
        // IMPORTANTE: El CVV no se almacena en la transacción
        // Solo se valida y se descarta (PCI-DSS compliance)
    }
    public Result Approve()
    {
        if (Status != TransactionStatus.Pending)
            return Result.Failure($"Cannot approve transaction in {Status} status");
        
        Status = TransactionStatus.Approved;
        return Result.Success();
    }
    
    public Result Decline(string reason)
    {
        if (Status != TransactionStatus.Pending)
            return Result.Failure($"Cannot decline transaction in {Status} status");
        
        Status = TransactionStatus.Declined;
        return Result.Success();
    }
}

public enum TransactionStatus
{
    Pending,
    Approved,
    Declined,
    Refunded
}