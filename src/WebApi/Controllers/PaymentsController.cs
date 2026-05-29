// src/WebApi/Controllers/PaymentsController.cs
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Features.Payments.Commands;
using Application.Features.Payments.Queries;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;  // ✅ Campo declarado
    
    // ✅ Constructor que recibe e inicializa _mediator
    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    private string MerchantId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                                   User.FindFirst("MerchantId")?.Value ?? 
                                   "unknown";
    
    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
    {
        if (request.MerchantId != MerchantId)
            return Unauthorized(new { message = "Merchant ID mismatch" });
        
        var command = new ProcessPaymentCommand
        {
            CardNumber = request.CardNumber,
            Cvv = request.Cvv,
            Amount = request.Amount,
            Currency = request.Currency,
            MerchantId = request.MerchantId,
            IdempotencyKey = request.IdempotencyKey
        };
        
        var result = await _mediator.Send(command);  // ✅ Ahora funciona
        return Ok(result);
    }
    
    [HttpGet("status/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentResponse>> GetTransactionStatus(Guid id)
    {
        var query = new GetTransactionStatusQuery { TransactionId = id };
        var result = await _mediator.Send(query);  // ✅ Ahora funciona
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}