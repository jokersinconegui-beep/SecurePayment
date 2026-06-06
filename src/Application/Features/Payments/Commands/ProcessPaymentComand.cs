// src/Application/Features/Payments/Commands/ProcessPaymentCommand.cs
using MediatR;
using Application.DTOs;
using Application.DTOs.Response;

namespace Application.Features.Payments.Commands;

public class ProcessPaymentCommand : IRequest<PaymentResponse>
{
    public string CardNumber { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string MerchantId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}

