// tests/IntegrationTests/RateLimitingTests.cs
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    
    public RateLimitingTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    private async Task AuthenticateAsync()
    {
        var registerRequest = new
        {
            merchantId = "RATELIMIT001",
            name = "Rate Limit Test",
            email = "ratelimit@test.com",
            password = "Password123!"
        };
        
        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");
        
        await _client.PostAsync("/api/Auth/register", registerContent);
        
        var loginRequest = new
        {
            email = "ratelimit@test.com",
            password = "Password123!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");
        
        var loginResponse = await _client.PostAsync("/api/Auth/login", loginContent);
        var loginResult = JsonSerializer.Deserialize<Dictionary<string, string>>(
            await loginResponse.Content.ReadAsStringAsync());
        
        _authToken = loginResult?["token"] ?? string.Empty;
    }
    
    [Fact]
    public async Task RateLimiting_BasicPlan_BlocksAfter100Requests()
    {
        // Arrange
        await AuthenticateAsync();
        
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        
        var successCount = 0;
        var blockedCount = 0;
        
        // Act - Realizar 105 requests rápidas
        for (int i = 0; i < 105; i++)
        {
            var response = await _client.GetAsync("/api/Payments/transactions?page=1&pageSize=1");
            
            if (response.StatusCode == HttpStatusCode.OK)
                successCount++;
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                blockedCount++;
        }
        
        // Assert
        successCount.Should().Be(100);
        blockedCount.Should().Be(5);
    }
}