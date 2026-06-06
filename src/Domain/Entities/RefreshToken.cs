// src/Domain/Entities/RefreshToken.cs
namespace Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string MerchantId { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    
    private RefreshToken() { }
    
    public RefreshToken(string merchantId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        MerchantId = merchantId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
        CreatedAt = DateTime.UtcNow;
    }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    public void Revoke(string? revokedByIp = null)
    {
        IsRevoked = true;
        RevokedByIp = revokedByIp;
    }
}