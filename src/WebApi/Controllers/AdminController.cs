// src/WebApi/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]  // Solo administradores
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPut("merchants/{merchantId}/plan")]
    public async Task<IActionResult> UpdateMerchantPlan(string merchantId, [FromBody] UpdatePlanRequest request)
    {
        var merchant = await _context.Merchants
            .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
        
        if (merchant == null)
            return NotFound(new { message = "Merchant not found" });
        
        if (request.Plan == "Premium")
            merchant.UpgradeToPremium();
        else
            merchant.DowngradeToBasic();
        
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            message = $"Merchant {merchantId} plan updated to {merchant.Plan}",
            rateLimit = merchant.GetRateLimit()
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
    public string Plan { get; set; } = string.Empty; // "Basic" o "Premium"
}