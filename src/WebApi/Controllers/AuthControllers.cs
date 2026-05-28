// src/WebApi/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;
using Application.DTOs.Auth;

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
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.AuthenticateAsync(request.Email, request.Password);
        
        if (response == null)
            return Unauthorized(new { message = "Invalid email or password" });
        
        return Ok(response);
    }
}