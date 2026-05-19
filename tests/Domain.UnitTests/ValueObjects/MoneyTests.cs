// tests/Domain.UnitTests/ValueObjects/MoneyTests.cs

using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ReturnsSuccess()
    {
        // Act
        var result = Money.Create(100.50m, "USD");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100.50m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }
    
    [Fact]
    public void Create_WithUnsupportedCurrency_ReturnsFailure()
    {
        // Act
        var result = Money.Create(100m, "XYZ");
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not supported", result.Error);
    }
    
    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        // Act
        var result = Money.Create(100.555m, "USD");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100.56m, result.Value.Amount); // Redondeado
    }
    
    // tests/Domain.UnitTests/ValueObjects/MoneyTests.cs

[Fact]
public void Create_WithTooManyDecimals_RoundsAutomatically()
{
    // Arrange - 4 decimales
    var result = Money.Create(100.5555m, "USD");
    
    // Assert - Debe tener éxito y redondear a 2 decimales
    Assert.True(result.IsSuccess);
    Assert.Equal(100.56m, result.Value.Amount); // Redondeado
}
    [Fact]
    public void Add_TwoSameCurrencies_ReturnsSum()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD").Value;
        var money2 = Money.Create(50.50m, "USD").Value;
        
        // Act
        var result = money1.Add(money2);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(150.50m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }
    
    [Fact]
    public void Add_DifferentCurrencies_ReturnsFailure()
    {
        // Arrange
        var usd = Money.Create(100m, "USD").Value;
        var eur = Money.Create(100m, "EUR").Value;
        
        // Act
        var result = usd.Add(eur);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Cannot add different currencies", result.Error);
    }
    
    [Fact]
    public void Subtract_TwoSameCurrencies_ReturnsDifference()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD").Value;
        var money2 = Money.Create(30m, "USD").Value;
        
        // Act
        var result = money1.Subtract(money2);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(70m, result.Value.Amount);
    }
    
    [Fact]
    public void Multiply_ByFactor_ReturnsProduct()
    {
        // Arrange
        var money = Money.Create(100m, "USD").Value;
        
        // Act
        var result = money.Multiply(1.5m);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.Amount);
    }
    
    [Fact]
    public void Divide_ByDivisor_ReturnsQuotient()
    {
        // Arrange
        var money = Money.Create(100m, "USD").Value;
        
        // Act
        var result = money.Divide(4m);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25m, result.Value.Amount);
    }
    
    [Fact]
    public void Compare_IsGreaterThan_ReturnsCorrectResult()
    {
        // Arrange
        var large = Money.Create(100m, "USD").Value;
        var small = Money.Create(50m, "USD").Value;
        
        // Act & Assert
        Assert.True(large.IsGreaterThan(small));
        Assert.False(small.IsGreaterThan(large));
    }
    
    [Fact]
    public void Equals_TwoDifferentCurrencies_ReturnsFalse()
    {
        // Arrange
        var usd = Money.Create(100m, "USD").Value;
        var eur = Money.Create(100m, "EUR").Value;
        
        // Act & Assert
        Assert.False(usd.Equals(eur));
        Assert.False(usd == eur);
    }
    
    [Fact]
    public void Equals_TwoSameValue_ReturnsTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD").Value;
        var money2 = Money.Create(100m, "USD").Value;
        
        // Act & Assert
        Assert.True(money1.Equals(money2));
        Assert.True(money1 == money2);
    }
    
    [Fact]
    public void Format_ReturnsCorrectString()
    {
        // Arrange
        var money = Money.Create(1234.56m, "USD").Value;
        
        // Act
        var formatted = money.Format("us");
        
        // Assert
        Assert.Equal("USD 1,234.56", formatted);
    }
    
    [Fact]
    public void Zero_CreatesZeroMoney()
    {
        // Act
        var zero = Money.Zero("USD");
        
        // Assert
        Assert.Equal(0m, zero.Amount);
        Assert.Equal("USD", zero.Currency);
    }
    
    [Fact]
    public void Usd_StaticFactory_CreatesUsdMoney()
    {
        // Act
        var result = Money.Usd(99.99m);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }
    
    [Fact]
    public void OperatorOverloading_WorksWithResultPattern()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD").Value;
        var money2 = Money.Create(50m, "USD").Value;
        
        // Act
        var sum = money1 + money2;  // Usa el operador +
        
        // Assert
        Assert.True(sum.IsSuccess);
        Assert.Equal(150m, sum.Value.Amount);
    }
}