// src/Application/Features/Payments/Commands/ProcessPaymentCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Domain.ValueObjects;
using Domain.Entities;
using Application.Common.Interfaces;

namespace Application.Features.Payments.Commands;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
{
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly IPaymentRepository _paymentRepository;

    public ProcessPaymentCommandHandler(
        ILogger<ProcessPaymentCommandHandler> logger,
        IPaymentRepository paymentRepository)
    {
        _logger = logger;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for merchant: {MerchantId}, Amount: {Amount} {Currency}",
            request.MerchantId, request.Amount, request.Currency);

        // 1. Validar CardNumber
        var cardResult = CardNumber.Create(request.CardNumber);
        if (cardResult.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = cardResult.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 2. Validar CVV
        var cvvResult = Cvv.Create(request.Cvv);
        if (cvvResult.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = cvvResult.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 3. Validar CVV contra marca de tarjeta
        var brandValidation = cvvResult.Value.ValidateForBrand(cardResult.Value.GetBrand());
        if (brandValidation.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = brandValidation.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 4. Validar Money
        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = moneyResult.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 5. Crear transacción
        var transactionResult = Transaction.Create(
        cardResult.Value,
        moneyResult.Value,
        cvvResult.Value,
        request.MerchantId,
        request.IdempotencyKey
    );
        if (transactionResult.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = transactionResult.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 6. Aprobar transacción (simular procesamiento)
        var approval = transactionResult.Value.Approve();
        if (approval.IsFailure)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = approval.Error,
                Timestamp = DateTime.UtcNow
            };
        }

        // 7. Guardar transacción (simulado)
        await _paymentRepository.SaveAsync(transactionResult.Value, cancellationToken);

        _logger.LogInformation("Payment approved for transaction: {TransactionId}", transactionResult.Value.Id);

        return new PaymentResponse
        {
            TransactionId = transactionResult.Value.Id,
            Status = "Approved",
            Message = "Payment processed successfully",
            Timestamp = DateTime.UtcNow,
            ApprovalCode = Guid.NewGuid().ToString()[..8].ToUpper()
        };
    }
}