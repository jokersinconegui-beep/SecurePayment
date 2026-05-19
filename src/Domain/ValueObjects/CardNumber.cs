using Domain.Common;

namespace Domain.ValueObjects;

public class CardNumber : IEquatable<CardNumber>
{
    public string Value { get; }
    public string Masked => $"****-****-****-{Value[^4..]}";
    public string LastFour => Value[^4..];
    public string Bin => Value[..6];
    
    private CardNumber(string value)
    {
        Value = value;
    }
    
    public static Result<CardNumber> Create(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result<CardNumber>.Failure("Card number is required");
        
        var cleaned = new string(number.Where(char.IsDigit).ToArray());
        
        if (cleaned.Length is < 14 or > 19)
            return Result<CardNumber>.Failure("Card number must be 14-19 digits");
        
        if (!IsValidLuhn(cleaned))
            return Result<CardNumber>.Failure("Invalid card number (Luhn check failed)");
        
        if (!IsValidBin(cleaned[..6]))
            return Result<CardNumber>.Failure($"Unknown or invalid card BIN: {cleaned[..6]}");
        
        return Result<CardNumber>.Success(new CardNumber(cleaned));
    }
    
    private static bool IsValidLuhn(string number)
    {
        int sum = 0;
        bool alternate = false;
        
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';
            
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit = (digit % 10) + 1;
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        return sum % 10 == 0;
    }
    
    private static bool IsValidBin(string bin)
    {
        // BINs conocidos (primeros dígitos)
        var validPrefixes = new[] { "4", "51", "52", "53", "54", "55", "34", "37", "6011", "65" };
        
        return validPrefixes.Any(prefix => bin.StartsWith(prefix));
    }
    
    public CardBrand GetBrand()
    {
        return Value[0] switch
        {
            '4' => CardBrand.Visa,
            '5' => CardBrand.Mastercard,
            '3' when Value[1] is '4' or '7' => CardBrand.AmericanExpress,
            '6' => CardBrand.Discover,
            _ => CardBrand.Unknown
        };
    }
    
    public bool Equals(CardNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as CardNumber);
    }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public override string ToString() => Masked;
}

public enum CardBrand
{
    Unknown,
    Visa,
    Mastercard,
    AmericanExpress,
    Discover
}