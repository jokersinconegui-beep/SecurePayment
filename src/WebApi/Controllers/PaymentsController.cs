// src/WebApi/Controllers/PaymentsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Payments.Commands;
using Application.Features.Payments.Queries;
using Application.DTOs;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;
    
    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
    {
        var command = new ProcessPaymentCommand
        {
            CardNumber = request.CardNumber,
            Cvv = request.Cvv,
            Amount = request.Amount,
            Currency = request.Currency,
            MerchantId = request.MerchantId,
            IdempotencyKey = request.IdempotencyKey
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    [HttpGet("status/{id}")]
    public async Task<ActionResult<PaymentResponse>> GetTransactionStatus(Guid id)
    {
        var query = new GetTransactionStatusQuery { TransactionId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}