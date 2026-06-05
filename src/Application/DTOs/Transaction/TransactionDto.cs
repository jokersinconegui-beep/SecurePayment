// src/Application/DTOs/Transactions/TransactionDto.cs
namespace Application.DTOs.Transactions;

public class TransactionDto
{
    public Guid TransactionId { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ApprovalCode { get; set; }
}