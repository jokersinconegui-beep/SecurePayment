// tests/IntegrationTests/AuthTests.cs
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static int _testCounter = 0;
    
    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    private string GetUniqueId() => $"TEST{Interlocked.Increment(ref _testCounter)}{DateTime.Now.Ticks}";
    
    [Fact]
    public async Task Register_ValidMerchant_ReturnsOk()
    {
        // Arrange - Usar datos únicos
        var uniqueId = GetUniqueId();
        var request = new
        {
            merchantId = $"MERCH_{uniqueId}",
            name = "Test Merchant",
            email = $"test_{uniqueId}@example.com",
            password = "Password123!"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("registered successfully");
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange - Registrar un merchant único primero
        var uniqueId = GetUniqueId();
        var merchantId = $"MERCH_{uniqueId}";
        var email = $"test_{uniqueId}@example.com";
        
        var registerRequest = new
        {
            merchantId = merchantId,
            name = "Test Merchant",
            email = email,
            password = "Password123!"
        };
        
        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");
        
        await _client.PostAsync("/api/Auth/register", registerContent);
        
        var loginRequest = new
        {
            email = email,
            password = "Password123!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/Auth/login", loginContent);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("token");
        responseContent.Should().Contain("refreshToken");
    }
    
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            email = "nonexistent@example.com",
            password = "WrongPassword!"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}