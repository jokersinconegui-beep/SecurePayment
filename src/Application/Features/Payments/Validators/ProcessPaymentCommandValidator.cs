// src/Application/Features/Payments/Validators/ProcessPaymentCommandValidator.cs
using FluentValidation;
using Application.Features.Payments.Commands;

namespace Application.Features.Payments.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .Matches(@"^\d{14,19}$").WithMessage("Card number must be 14-19 digits");
        
        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required")
            .Matches(@"^\d{3,4}$").WithMessage("CVV must be 3 or 4 digits");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThan(1000000).WithMessage("Amount exceeds maximum allowed");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(c => new[] { "USD", "EUR", "GBP", "COP", "MXN" }.Contains(c))
            .WithMessage("Currency not supported");
        
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required");
        
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required");
    }
}