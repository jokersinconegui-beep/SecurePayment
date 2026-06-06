// src/WebApi/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Application.DTOs.Request;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.MerchantId,
            request.Name,
            request.Email,
            request.Password);

        if (!result)
            return BadRequest(new { message = "Registration failed. Email or Merchant ID already exists." });

        return Ok(new { message = "Merchant registered successfully" });
    }

    // src/WebApi/Controllers/AuthController.cs
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await _authService.AuthenticateAsync(request.Email, request.Password, ipAddress);

        if (response == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(response);
    }

    // src/WebApi/Controllers/AuthController.cs (agregar método)

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (response == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);

        return Ok(new { message = "Logged out successfully" });
    }
}