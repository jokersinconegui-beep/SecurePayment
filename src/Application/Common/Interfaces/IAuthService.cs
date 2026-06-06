// src/Application/Common/Interfaces/IAuthService.cs
using Application.DTOs.Response;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(string merchantId, string name, string email, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateJwtToken(string merchantId, string email, string name);
    Task<AuthResponse?> AuthenticateAsync(string email, string password, string ipAddress);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
}