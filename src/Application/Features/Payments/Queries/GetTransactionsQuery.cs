// src/Application/Features/Payments/Queries/GetTransactionsQuery.cs
using MediatR;
using Application.DTOs.Transactions;

namespace Application.Features.Payments.Queries;

public class GetTransactionsQuery : IRequest<TransactionListResponse>
{
    public string MerchantId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}