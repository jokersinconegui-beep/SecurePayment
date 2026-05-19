// src/Application/Features/Payments/Queries/GetTransactionStatusQuery.cs
using MediatR;
using Application.DTOs;

namespace Application.Features.Payments.Queries;

public class GetTransactionStatusQuery : IRequest<PaymentResponse?>
{
    public Guid TransactionId { get; set; }
}