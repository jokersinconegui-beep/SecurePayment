// tests/Domain.UnitTests/ValueObjects/CvvTests.cs
using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests.ValueObjects;

public class CvvTests
{
    [Fact]
    public void Create_WithValid3DigitCvv_ReturnsSuccess()
    {
        // Arrange & Act
        var result = Cvv.Create("123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("123", result.Value.Value);
    }

    [Fact]
    public void Create_WithValid4DigitCvv_ReturnsSuccess()
    {
        // Act
        var result = Cvv.Create("1234");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("1234", result.Value.Value);
    }

    [Fact]
    public void Create_WithSpaces_TrimsAndWorks()
    {
        // Act
        var result = Cvv.Create(" 123 ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("123", result.Value.Value);
    }

    [Fact]
    public void Create_WithLetters_ReturnsFailure()
    {
        // Act
        var result = Cvv.Create("abc");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("only digits", result.Error);
    }

    [Fact]
    public void Create_WithInvalidLength_ReturnsFailure()
    {
        // Act - 2 dígitos
        var result1 = Cvv.Create("12");

        // Act - 5 dígitos
        var result2 = Cvv.Create("12345");

        // Assert
        Assert.True(result1.IsFailure);
        Assert.True(result2.IsFailure);
        Assert.Contains("3 or 4 digits", result1.Error);
    }

    [Fact]
    public void Create_WithAllZeros_ReturnsFailure()
    {
        // Act
        var result1 = Cvv.Create("000");
        var result2 = Cvv.Create("0000");

        // Assert
        Assert.True(result1.IsFailure);
        Assert.True(result2.IsFailure);
        Assert.Contains("Invalid CVV", result1.Error);
    }

    [Fact]
    public void Create_WithEmptyString_ReturnsFailure()
    {
        // Act
        var result = Cvv.Create("");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("required", result.Error);
    }

    [Fact]
    public void ValidateForBrand_AmexRequires4Digits()
    {
        // Arrange
        var cvv3 = Cvv.Create("123").Value;
        var cvv4 = Cvv.Create("1234").Value;

        // Act & Assert
        Assert.True(cvv4.ValidateForBrand(CardBrand.AmericanExpress).IsSuccess);
        Assert.True(cvv3.ValidateForBrand(CardBrand.AmericanExpress).IsFailure);
        Assert.Contains("4 digits", cvv3.ValidateForBrand(CardBrand.AmericanExpress).Error);
    }

    [Fact]
    public void ValidateForBrand_VisaRequires3Digits()
    {
        // Arrange
        var cvv3 = Cvv.Create("123").Value;
        var cvv4 = Cvv.Create("1234").Value;

        // Act & Assert
        Assert.True(cvv3.ValidateForBrand(CardBrand.Visa).IsSuccess);
        Assert.True(cvv4.ValidateForBrand(CardBrand.Visa).IsFailure);
        Assert.Contains("3 digits", cvv4.ValidateForBrand(CardBrand.Visa).Error);
    }

    [Fact]
    public void Matches_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var cvv = Cvv.Create("123").Value;

        // Act & Assert
        Assert.True(cvv.Matches("123"));
    }

    [Fact]
    public void Matches_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var cvv = Cvv.Create("123").Value;

        // Act & Assert
        Assert.False(cvv.Matches("456"));
    }

    [Fact]
    public void Matches_WithNull_ReturnsFalse()
    {
        // Arrange
        var cvv = Cvv.Create("123").Value;

        // Act & Assert
        Assert.False(cvv.Matches(null));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var cvv1 = Cvv.Create("123").Value;
        var cvv2 = Cvv.Create("123").Value;

        // Act & Assert
        Assert.True(cvv1.Equals(cvv2));
        Assert.True(cvv1 == cvv2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var cvv1 = Cvv.Create("123").Value;
        var cvv2 = Cvv.Create("456").Value;

        // Act & Assert
        Assert.False(cvv1.Equals(cvv2));
    }

    [Fact]
    public void ToString_ReturnsEmptyOrMasked_ForSecurity()
    {
        // Arrange
        var cvv = Cvv.Create("123").Value;

        // Act
        var toStringResult = cvv.ToString();

        // Assert - ToString no debería revelar el CVV
        // (dependiendo de tu implementación, puede ser vacío o "***")
        Assert.DoesNotContain("123", toStringResult);
    }
}