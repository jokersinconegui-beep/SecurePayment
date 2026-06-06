// src/Application/DTOs/PaymentRequest.cs
namespace Application.DTOs.Request;

public class PaymentRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string MerchantId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}

