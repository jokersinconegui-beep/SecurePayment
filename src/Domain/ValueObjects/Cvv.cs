// src/Domain/ValueObjects/Cvv.cs
using Domain.Common;

namespace Domain.ValueObjects;

/// <summary>
/// Representa el CVV/CVC de una tarjeta de crédito.
/// NUNCA se almacena, solo se valida en el momento de la transacción.
/// </summary>
public class Cvv : IEquatable<Cvv>
{
    public string Value { get; }
    
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
    
    private Cvv(string value)
    {
        Value = value;
    }
    
    public static Result<Cvv> Create(string? cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            return Result<Cvv>.Failure("CVV is required");
        
        var cleaned = cvv.Trim();
        
        if (!cleaned.All(char.IsDigit))
            return Result<Cvv>.Failure("CVV must contain only digits");
        
        if (cleaned.Length is not 3 and not 4)
            return Result<Cvv>.Failure("CVV must be 3 or 4 digits");
        
        if (cleaned.All(c => c == '0'))
            return Result<Cvv>.Failure("Invalid CVV");
        
        return Result<Cvv>.Success(new Cvv(cleaned));
    }
    
    public Result ValidateForBrand(CardBrand brand)
    {
        return brand switch
        {
            CardBrand.AmericanExpress => Value.Length == 4 
                ? Result.Success() 
                : Result.Failure("American Express CVV must be 4 digits"),
            
            CardBrand.Visa or CardBrand.Mastercard or CardBrand.Discover => Value.Length == 3
                ? Result.Success()
                : Result.Failure($"{brand} CVV must be 3 digits"),
            
            _ => Result.Failure("Unknown card brand for CVV validation")
        };
    }
    
    public bool Matches(string? cvvToCompare)
    {
        if (string.IsNullOrWhiteSpace(cvvToCompare))
            return false;
        
        return CryptographicCompare(Value, cvvToCompare.Trim());
    }
    
    private static bool CryptographicCompare(string a, string b)
    {
        if (a.Length != b.Length)
            return false;
        
        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
    
    // ✅ Implementación corregida de Equals
    public bool Equals(Cvv? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as Cvv);
    }
    
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }
    
    // ✅ Implementar operadores para que funcione correctamente
    public static bool operator ==(Cvv? left, Cvv? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    
    public static bool operator !=(Cvv? left, Cvv? right) => !(left == right);
    
    // ToString no debe revelar el CVV
    public override string ToString() => "***";
    
    // Método interno para debug (no usar en producción)
    internal string ToDebugString() => $"CVV:[{new string('*', Value.Length)}]";
}