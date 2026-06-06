// src/Application/Features/Payments/Queries/GetTransactionStatusQuery.cs
using MediatR;
using Application.DTOs;
using Application.DTOs.Response;

namespace Application.Features.Payments.Queries;

public class GetTransactionStatusQuery : IRequest<PaymentResponse?>
{
    public Guid TransactionId { get; set; }
}