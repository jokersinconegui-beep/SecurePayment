// src/Infrastructure/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Interfaces;
using Application.DTOs.Response;
using Domain.Entities;
using Infrastructure.Persistence;
using System.Security.Cryptography;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }



    public async Task<bool> RegisterAsync(string merchantId, string name, string email, string password)
    {
        // Verificar si ya existe
        var exists = await _context.Merchants
            .AnyAsync(m => m.Email == email || m.MerchantId == merchantId);

        if (exists)
            return false;

        var passwordHash = HashPassword(password);
        var merchant = Merchant.Create(merchantId, name, email, passwordHash);

        if (merchant.IsFailure)
            return false;

        await _context.Merchants.AddAsync(merchant.Value);
        await _context.SaveChangesAsync();

        return true;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateJwtToken(string merchantId, string email, string name)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "SuperSecretKey1234567890SuperSecretKey"));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, merchantId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim("MerchantId", merchantId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SecurePaymentGateway",
            audience: _configuration["Jwt:Audience"] ?? "SecurePaymentGateway",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    // src/Infrastructure/Services/AuthService.cs (agregar métodos)

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        // Buscar refresh token en BD
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (storedToken == null || storedToken.IsExpired)
            return null;

        // Obtener merchant
        var merchant = await _context.Merchants
            .FirstOrDefaultAsync(m => m.MerchantId == storedToken.MerchantId && m.IsActive);

        if (merchant == null)
            return null;

        // Revocar el refresh token usado
        storedToken.Revoke(ipAddress);

        // Generar nuevo token
        var newJwtToken = GenerateJwtToken(merchant.MerchantId, merchant.Email, merchant.Name);
        var newRefreshToken = GenerateRefreshToken(merchant.MerchantId);

        // Guardar nuevo refresh token
        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken.Token,
            MerchantId = merchant.MerchantId,
            Name = merchant.Name,
            Email = merchant.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            return false;

        storedToken.Revoke(ipAddress);
        await _context.SaveChangesAsync();

        return true;
    }

private RefreshToken GenerateRefreshToken(string merchantId)
{
    // ✅ Generar un token único de 64 caracteres
    var randomBytes = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(randomBytes);
    }
    var token = Convert.ToBase64String(randomBytes)
        .Replace("+", "")
        .Replace("/", "")
        .Replace("=", "");
    
    return new RefreshToken(merchantId, token, DateTime.UtcNow.AddDays(7));
}

    // src/Infrastructure/Services/AuthService.cs - AuthenticateAsync
    // src/Infrastructure/Services/AuthService.cs
public async Task<AuthResponse?> AuthenticateAsync(string email, string password, string ipAddress)
{
    var merchant = await _context.Merchants
        .FirstOrDefaultAsync(m => m.Email == email && m.IsActive);
    
    if (merchant == null || !VerifyPassword(password, merchant.PasswordHash))
        return null;
    
    merchant.RecordLogin();
    await _context.SaveChangesAsync();  // ✅ Guardar cambios del login
    
    var jwtToken = GenerateJwtToken(merchant.MerchantId, merchant.Email, merchant.Name);
    
    // ✅ Generar Refresh Token
    var refreshToken = GenerateRefreshToken(merchant.MerchantId);
    
    // ✅ Guardar Refresh Token en BD
    await _context.RefreshTokens.AddAsync(refreshToken);
    await _context.SaveChangesAsync();
    
    // ✅ Verificar que refreshToken.Token NO está vacío
    if (string.IsNullOrEmpty(refreshToken.Token))
        throw new InvalidOperationException("Refresh token was not generated properly");
    
    return new AuthResponse
    {
        Token = jwtToken,
        RefreshToken = refreshToken.Token,  // ← Asegurar que esto tiene valor
        MerchantId = merchant.MerchantId,
        Name = merchant.Name,
        Email = merchant.Email,
        ExpiresAt = DateTime.UtcNow.AddHours(1)
    };
}
}