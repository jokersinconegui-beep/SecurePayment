using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests.ValueObjects;

public class CardNumberTests
{
    [Fact]
    public void Create_WithValidVisaNumber_ReturnsSuccess()
    {
        // Arrange
        string validVisa = "4532015112830366";
        
        // Act
        var result = CardNumber.Create(validVisa);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("4532015112830366", result.Value.Value);
        Assert.Equal("****-****-****-0366", result.Value.Masked);
        Assert.Equal(CardBrand.Visa, result.Value.GetBrand());
    }
    
    [Fact]
    public void Create_WithValidMastercardNumber_ReturnsSuccess()
    {
        // Arrange
        string validMastercard = "5555555555554444";
        
        // Act
        var result = CardNumber.Create(validMastercard);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CardBrand.Mastercard, result.Value.GetBrand());
    }
    
    [Fact]
    public void Create_WithInvalidLuhn_ReturnsFailure()
    {
        // Arrange
        string invalidCard = "4532015112830367"; // Último dígito incorrecto
        
        // Act
        var result = CardNumber.Create(invalidCard);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Luhn", result.Error);
    }
    
    [Fact]
    public void Create_WithSpacesAndDashes_WorksCorrectly()
    {
        // Arrange
        string withSpaces = "4532 0151 1283 0366";
        string withDashes = "4532-0151-1283-0366";
        
        // Act
        var result1 = CardNumber.Create(withSpaces);
        var result2 = CardNumber.Create(withDashes);
        
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(result1.Value.Value, result2.Value.Value);
    }
    
    [Fact]
    public void Masked_ShowsOnlyLastFour()
    {
        // Arrange
        var card = CardNumber.Create("4532015112830366").Value;
        
        // Assert
        Assert.Equal("****-****-****-0366", card.Masked);
        Assert.DoesNotContain("4532", card.Masked);
    }
    
    [Fact]
    public void Create_WithEmptyNumber_ReturnsFailure()
    {
        // Act
        var result = CardNumber.Create("");
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("required", result.Error);
    }
}