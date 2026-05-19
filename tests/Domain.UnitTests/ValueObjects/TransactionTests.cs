// tests/Domain.UnitTests/Entities/TransactionTests.cs
using Domain.Entities;
using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests.Entities;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidCardMoneyAndCvv_ReturnsSuccess()
    {
        // Arrange
        var card = CardNumber.Create("4532015112830366").Value;
        var amount = Money.Create(100m, "USD").Value;
        var cvv = Cvv.Create("123").Value;
        
        // Act
        var result = Transaction.Create(card, amount, cvv);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionStatus.Pending, result.Value.Status);
    }
    
    [Fact]
    public void Create_WithAmexCardAnd3DigitCvv_ReturnsFailure()
    {
        // Arrange - Amex requiere CVV de 4 dígitos
        var card = CardNumber.Create("378282246310005").Value; // Amex test
        var amount = Money.Create(100m, "USD").Value;
        var cvv = Cvv.Create("123").Value; // 3 dígitos, Amex necesita 4
        
        // Act
        var result = Transaction.Create(card, amount, cvv);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("4 digits", result.Error);
    }
    
    [Fact]
    public void Create_WithAmexCardAnd4DigitCvv_ReturnsSuccess()
    {
        // Arrange
        var card = CardNumber.Create("378282246310005").Value;
        var amount = Money.Create(100m, "USD").Value;
        var cvv = Cvv.Create("1234").Value;
        
        // Act
        var result = Transaction.Create(card, amount, cvv);
        
        // Assert
        Assert.True(result.IsSuccess);
    }
}