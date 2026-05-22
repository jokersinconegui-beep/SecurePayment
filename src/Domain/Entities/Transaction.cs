// src/Domain/Entities/Transaction.cs (versión actualizada)
using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public CardNumber CardNumber { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public string MerchantId { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public TransactionStatus Status { get; private set; }
    
    private Transaction() { }
    
    private Transaction(CardNumber cardNumber, Money amount, string merchantId, string idempotencyKey)
    {
        Id = Guid.NewGuid();
        CardNumber = cardNumber;
        Amount = amount;
        MerchantId = merchantId;
        IdempotencyKey = idempotencyKey;
        CreatedAt = DateTime.UtcNow;
        Status = TransactionStatus.Pending;
    }
    
    public static Result<Transaction> Create(CardNumber card, Money amount, Cvv cvv, string merchantId, string idempotencyKey)
    {
        if (!amount.IsPositive())
            return Result<Transaction>.Failure("Transaction amount must be positive");
        
        if (string.IsNullOrWhiteSpace(merchantId))
            return Result<Transaction>.Failure("Merchant ID is required");
        
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Result<Transaction>.Failure("Idempotency key is required");
        
        var brandValidation = cvv.ValidateForBrand(card.GetBrand());
        if (brandValidation.IsFailure)
            return Result<Transaction>.Failure(brandValidation.Error);
        
        var transaction = new Transaction(card, amount, merchantId, idempotencyKey);
        return Result<Transaction>.Success(transaction);
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