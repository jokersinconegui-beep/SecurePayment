// src/Domain/Entities/Merchant.cs
using Domain.Common;

namespace Domain.Entities;

public enum MerchantPlan
{
    Basic,      // 100 requests por minuto
    Premium     // 1000 requests por minuto
}


public class Merchant
{
    public Guid Id { get; private set; }
    public string MerchantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string ApiKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    private Merchant() { }
    
    private Merchant(string merchantId, string name, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        MerchantId = merchantId;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        ApiKey = GenerateApiKey();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public MerchantPlan Plan { get; private set; } = MerchantPlan.Basic;
    
    public int GetRateLimit()
    {
        return Plan switch
        {
            MerchantPlan.Basic => 100,
            MerchantPlan.Premium => 1000,
            _ => 100
        };
    }
    
    public void UpgradeToPremium()
    {
        Plan = MerchantPlan.Premium;
    }
    
    public void DowngradeToBasic()
    {
        Plan = MerchantPlan.Basic;
    }
    
    public static Result<Merchant> Create(string merchantId, string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(merchantId))
            return Result<Merchant>.Failure("Merchant ID is required");
        
        if (string.IsNullOrWhiteSpace(name))
            return Result<Merchant>.Failure("Name is required");
        
        if (string.IsNullOrWhiteSpace(email))
            return Result<Merchant>.Failure("Email is required");
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result<Merchant>.Failure("Password is required");
        
        return Result<Merchant>.Success(new Merchant(merchantId, name, email, passwordHash));
    }
    
    private static string GenerateApiKey()
    {
        // ✅ Método seguro - Guid.ToString("N") siempre genera 32 caracteres hexadecimales
        return Guid.NewGuid().ToString("N");
    }
    
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
}

