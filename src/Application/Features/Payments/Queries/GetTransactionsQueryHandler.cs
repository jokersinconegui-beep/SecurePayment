// src/Application/Features/Payments/Queries/GetTransactionsQueryHandler.cs
using MediatR;
using Application.Common.Interfaces;
using Application.DTOs.Transactions;

namespace Application.Features.Payments.Queries;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, TransactionListResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    
    public GetTransactionsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    
    public async Task<TransactionListResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetTransactionsByMerchantAsync(
            request.MerchantId,
            request.Page,
            request.PageSize,
            request.Status,
            request.FromDate,
            request.ToDate,
            cancellationToken);
    }
}