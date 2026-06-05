// src/Application/Features/Payments/Queries/GetTransactionStatusQueryHandler.cs
using MediatR;
using Application.Common.Interfaces;
using Application.DTOs;

namespace Application.Features.Payments.Queries;

public class GetTransactionStatusQueryHandler : IRequestHandler<GetTransactionStatusQuery, PaymentResponse?>
{
    private readonly IPaymentRepository _paymentRepository;
    
    public GetTransactionStatusQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    
    public async Task<PaymentResponse?> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _paymentRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        
        if (transaction == null)
            return null;
        
        return new PaymentResponse
        {
            TransactionId = transaction.Id,
            Status = transaction.Status.ToString(),
            Message = transaction.Status == Domain.Entities.TransactionStatus.Approved 
                ? "Payment approved" 
                : "Payment pending or declined",
            Timestamp = transaction.CreatedAt,
            ApprovalCode = transaction.Status == Domain.Entities.TransactionStatus.Approved 
                ? "APR" + transaction.Id.ToString()[..6] 
                : null
        };
    }
}