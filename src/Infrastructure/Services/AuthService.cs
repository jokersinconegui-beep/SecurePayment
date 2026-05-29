// src/Infrastructure/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services.Persistence;

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
    
    public async Task<AuthResponse?> AuthenticateAsync(string email, string password)
    {
        var merchant = await _context.Merchants
            .FirstOrDefaultAsync(m => m.Email == email && m.IsActive);
        
        if (merchant == null)
            return null;
        
        if (!VerifyPassword(password, merchant.PasswordHash))
            return null;
        
        merchant.RecordLogin();
        await _context.SaveChangesAsync();
        
        var token = GenerateJwtToken(merchant.MerchantId, merchant.Email, merchant.Name);
        
        return new AuthResponse
        {
            Token = token,
            MerchantId = merchant.MerchantId,
            Name = merchant.Name,
            Email = merchant.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
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
}