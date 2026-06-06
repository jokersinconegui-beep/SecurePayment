// src/Application/DTOs/Auth/RefreshTokenRequest.cs
namespace Application.DTOs.Request;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}