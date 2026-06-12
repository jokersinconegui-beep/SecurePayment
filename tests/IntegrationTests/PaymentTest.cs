// tests/IntegrationTests/PaymentTests.cs
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class PaymentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    
    public PaymentTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    private async Task AuthenticateAsync()
    {
        // Registrar y login para obtener token
        var registerRequest = new
        {
            merchantId = "PAYMENT001",
            name = "Payment Test",
            email = "payment@test.com",
            password = "Password123!"
        };
        
        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");
        
        await _client.PostAsync("/api/Auth/register", registerContent);
        
        var loginRequest = new
        {
            email = "payment@test.com",
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
    public async Task ProcessPayment_ValidCard_ReturnsApproved()
    {
        // Arrange
        await AuthenticateAsync();
        
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        
        var request = new
        {
            cardNumber = "4532015112830366",
            cvv = "123",
            amount = 99.99m,
            currency = "USD",
            merchantId = "PAYMENT001",
            idempotencyKey = Guid.NewGuid().ToString()
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/Payments/process", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("Approved");
        result.Should().Contain("transactionId");
    }
    
    [Fact]
    public async Task ProcessPayment_InvalidCard_ReturnsFailed()
    {
        // Arrange
        await AuthenticateAsync();
        
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        
        var request = new
        {
            cardNumber = "1111111111111111",  // Tarjeta inválida
            cvv = "123",
            amount = 99.99m,
            currency = "USD",
            merchantId = "PAYMENT001",
            idempotencyKey = Guid.NewGuid().ToString()
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/Payments/process", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("Failed");
    }
    
    [Fact]
    public async Task GetTransactions_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();
        
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        
        // Crear algunas transacciones primero
        for (int i = 0; i < 3; i++)
        {
            var paymentRequest = new
            {
                cardNumber = "4532015112830366",
                cvv = "123",
                amount = 50 + i,
                currency = "USD",
                merchantId = "PAYMENT001",
                idempotencyKey = Guid.NewGuid().ToString()
            };
            
            var content = new StringContent(
                JsonSerializer.Serialize(paymentRequest),
                Encoding.UTF8,
                "application/json");
            
            await _client.PostAsync("/api/Payments/process", content);
        }
        
        // Act
        var response = await _client.GetAsync("/api/Payments/transactions?page=1&pageSize=10");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("transactions");
        result.Should().Contain("totalCount");
    }
}