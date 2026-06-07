// src/WebApi/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.RateLimiters;

namespace WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]  // Requiere autenticación
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly MerchantRateLimiter _rateLimiter;
    
    public AdminController(ApplicationDbContext context, MerchantRateLimiter rateLimiter)
    {
        _context = context;
        _rateLimiter = rateLimiter;
    }
    
    [HttpPut("merchants/{merchantId}/plan")]
    public async Task<IActionResult> UpdateMerchantPlan(string merchantId, [FromBody] UpdatePlanRequest request)
    {
        var merchant = await _context.Merchants
            .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
        
        if (merchant == null)
            return NotFound(new { message = "Merchant not found" });
        
        // Actualizar plan
        if (request.Plan?.ToLower() == "premium")
            merchant.UpgradeToPremium();
        else
            merchant.DowngradeToBasic();
        
        await _context.SaveChangesAsync();
        
        // ✅ REFRESCAR CACHÉ DEL RATE LIMITER
        _rateLimiter.RefreshLimiter(merchantId);
        
        return Ok(new 
        { 
            message = $"Merchant {merchantId} plan updated to {merchant.Plan}",
            rateLimit = merchant.GetRateLimit(),
            plan = merchant.Plan.ToString()
        });
    }
    
    [HttpGet("merchants/{merchantId}/plan")]
    public async Task<IActionResult> GetMerchantPlan(string merchantId)
    {
        var merchant = await _context.Merchants
            .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
        
        if (merchant == null)
            return NotFound(new { message = "Merchant not found" });
        
        return Ok(new
        {
            merchantId = merchant.MerchantId,
            plan = merchant.Plan.ToString(),
            rateLimit = merchant.GetRateLimit()
        });
    }
}

public class UpdatePlanRequest
{
    public string Plan { get; set; } = string.Empty;
}