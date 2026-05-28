// src/Application/Common/Interfaces/IAuthService.cs
using Application.DTOs.Auth;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> AuthenticateAsync(string email, string password);
    Task<bool> RegisterAsync(string merchantId, string name, string email, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateJwtToken(string merchantId, string email, string name);
}