// tests/IntegrationTests/HealthCheckTests.cs
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task HealthLive_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
    
    [Fact]
    public async Task HealthReady_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
    
    [Fact]
    public async Task Health_ReturnsCompleteReport()
    {
        // Act
        var response = await _client.GetAsync("/health");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("database");
        content.Should().Contain("status");
        content.Should().NotContain("redis"); // ✅ Redis no está presente
    }
}