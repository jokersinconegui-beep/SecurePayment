// src/Application/Features/Payments/Commands/ProcessPaymentCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.ValueObjects;
using Domain.Entities;
using Application.Common.Interfaces;
using Application.DTOs.Response;

namespace Application.Features.Payments.Commands;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
{
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMetricsService _metrics;

    public ProcessPaymentCommandHandler(
        ILogger<ProcessPaymentCommandHandler> logger,
        IMetricsService metrics,
        IPaymentRepository paymentRepository)
    {
        _logger = logger;
        _metrics = metrics;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Log informativo con contexto
            _logger.LogInformation("Processing payment for merchant {MerchantId}, Amount: {Amount} {Currency}, IdempotencyKey: {IdempotencyKey}",
                request.MerchantId, request.Amount, request.Currency, request.IdempotencyKey);

            // 1. Validar CardNumber
            var cardResult = CardNumber.Create(request.CardNumber);
            if (cardResult.IsFailure)
            {
                _logger.LogWarning("Card validation failed for merchant {MerchantId}: {Error}",
                    request.MerchantId, cardResult.Error);
                return CreateFailedResponse(cardResult.Error);
            }

            _logger.LogDebug("Card validated successfully for merchant {MerchantId}, Brand: {Brand}",
                request.MerchantId, cardResult.Value.GetBrand());

            // 2. Validar CVV
            var cvvResult = Cvv.Create(request.Cvv);
            if (cvvResult.IsFailure)
            {
                _logger.LogWarning("CVV validation failed for merchant {MerchantId}: {Error}",
                    request.MerchantId, cvvResult.Error);
                return CreateFailedResponse(cvvResult.Error);
            }

            // 3. Validar CVV contra marca de tarjeta
            var brandValidation = cvvResult.Value.ValidateForBrand(cardResult.Value.GetBrand());
            if (brandValidation.IsFailure)
            {
                _logger.LogWarning("Brand validation failed for merchant {MerchantId}: {Error}",
                    request.MerchantId, brandValidation.Error);
                return CreateFailedResponse(brandValidation.Error);
            }

            // 4. Validar Money
            var moneyResult = Money.Create(request.Amount, request.Currency);
            if (moneyResult.IsFailure)
            {
                _logger.LogWarning("Money validation failed for merchant {MerchantId}: {Error}",
                    request.MerchantId, moneyResult.Error);
                return CreateFailedResponse(moneyResult.Error);
            }

            // 5. Verificar idempotencia (evitar dobles cargos)
            var exists = await _paymentRepository.ExistsByKeyAsync(request.IdempotencyKey, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Duplicate payment attempt detected for merchant {MerchantId}, IdempotencyKey: {IdempotencyKey}",
                    request.MerchantId, request.IdempotencyKey);
                return CreateFailedResponse("Duplicate transaction detected. Please check your previous request.");
            }

            // 6. Crear transacción
            var transactionResult = Transaction.Create(
                cardResult.Value,
                moneyResult.Value,
                cvvResult.Value,
                request.MerchantId,
                request.IdempotencyKey
            );

            if (transactionResult.IsFailure)
            {
                _logger.LogError("Transaction creation failed for merchant {MerchantId}: {Error}",
                    request.MerchantId, transactionResult.Error);
                return CreateFailedResponse(transactionResult.Error);
            }

            // 7. Aprobar transacción (simular procesamiento externo)
            var approval = transactionResult.Value.Approve();
            if (approval.IsFailure)
            {
                _logger.LogError("Payment gateway rejected transaction for merchant {MerchantId}: {Error}",
                    request.MerchantId, approval.Error);
                return CreateFailedResponse(approval.Error);
            }

            // 8. Guardar transacción en base de datos
            await _paymentRepository.SaveAsync(transactionResult.Value, cancellationToken);

            stopwatch.Stop();
            // ✅ Registrar métricas de éxito
            _metrics.RecordPaymentProcessed(request.MerchantId, "success", (double)request.Amount);
            _metrics.RecordPaymentDuration(request.MerchantId, stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Payment approved for merchant {MerchantId}, TransactionId: {TransactionId}, Amount: {Amount}, Time: {Elapsed}ms",
                request.MerchantId, transactionResult.Value.Id, request.Amount, stopwatch.ElapsedMilliseconds);

            return new PaymentResponse
            {
                TransactionId = transactionResult.Value.Id,
                Status = "Approved",
                Message = "Payment processed successfully",
                Timestamp = DateTime.UtcNow,
                ApprovalCode = GenerateApprovalCode()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // ✅ Registrar métricas de error
            _metrics.RecordPaymentProcessed(request.MerchantId, "error", 0);
            _metrics.RecordPaymentDuration(request.MerchantId, stopwatch.ElapsedMilliseconds);
            _logger.LogError(ex, "Unexpected error processing payment for merchant {MerchantId}, Amount: {Amount}, Time: {Elapsed}ms",
                request.MerchantId, request.Amount, stopwatch.ElapsedMilliseconds);

            return CreateFailedResponse("An unexpected error occurred. Please try again later.");
        }
    }

    private static PaymentResponse CreateFailedResponse(string message)
    {
        return new PaymentResponse
        {
            Status = "Failed",
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    private static string GenerateApprovalCode()
    {
        return Guid.NewGuid().ToString()[..8].ToUpper();
    }
}