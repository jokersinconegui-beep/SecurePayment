// Domain/ValueObjects/Money.cs
namespace Domain.ValueObjects;

using Domain.Common;


/// <summary>
/// Representa una cantidad de dinero con moneda específica.
/// Inmutable y evita problemas de precisión con double/decimal.
/// </summary>
public class Money : IEquatable<Money>
{
    // Propiedades
    public decimal Amount { get; }
    public string Currency { get; }

    // Monedas soportadas (ISO 4217)
    public static readonly string[] SupportedCurrencies = { "USD", "EUR", "GBP", "COP", "MXN" };

    // Constantes para validaciones
    private const int MaxDecimalPlaces = 2;
    private const decimal MinAmount = -1_000_000m;
    private const decimal MaxAmount = 1_000_000m;

    // Constructor privado (forzar creación vía factory)
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Crea una nueva instancia de Money con validaciones
    /// </summary>
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure("Currency is required");

        var normalizedCurrency = currency.ToUpperInvariant();
        if (!SupportedCurrencies.Contains(normalizedCurrency))
            return Result<Money>.Failure($"Currency '{currency}' not supported");

        if (amount < MinAmount || amount > MaxAmount)
            return Result<Money>.Failure($"Amount must be between {MinAmount} and {MaxAmount}");

        // ✅ Redondear a 2 decimales (no rechazar, redondear automáticamente)
        var roundedAmount = Math.Round(amount, MaxDecimalPlaces, MidpointRounding.AwayFromZero);

        // ✅ Eliminar o modificar esta validación (opcional: agregar warning en log)
        // Simplemente usamos el valor redondeado sin rechazar

        return Result<Money>.Success(new Money(roundedAmount, normalizedCurrency));
    }
    /// <summary>
    /// Crea Money en Cero
    /// </summary>
    public static Money Zero(string currency) => new(0m, currency.ToUpperInvariant());

    /// <summary>
    /// Crea Money en USD (conveniencia)
    /// </summary>
    public static Result<Money> Usd(decimal amount) => Create(amount, "USD");

    /// <summary>
    /// Crea Money en EUR (conveniencia)
    /// </summary>
    public static Result<Money> Eur(decimal amount) => Create(amount, "EUR");

    // Operaciones matemáticas (siempre devuelven nuevos objetos, no mutan)

    /// <summary>
    /// Suma dos cantidades de dinero (misma moneda)
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Failure($"Cannot add different currencies: {Currency} and {other.Currency}");

        var newAmount = Amount + other.Amount;
        return Create(newAmount, Currency);
    }

    /// <summary>
    /// Resta dos cantidades de dinero (misma moneda)
    /// </summary>
    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Failure($"Cannot subtract different currencies: {Currency} and {other.Currency}");

        var newAmount = Amount - other.Amount;
        return Create(newAmount, Currency);
    }

    /// <summary>
    /// Multiplica el dinero por un factor
    /// </summary>
    public Result<Money> Multiply(decimal factor)
    {
        if (factor < 0)
            return Result<Money>.Failure("Multiplication factor cannot be negative");

        var newAmount = Amount * factor;
        return Create(newAmount, Currency);
    }

    /// <summary>
    /// Divide el dinero por un divisor
    /// </summary>
    public Result<Money> Divide(decimal divisor)
    {
        if (divisor <= 0)
            return Result<Money>.Failure("Divisor must be greater than zero");

        var newAmount = Amount / divisor;
        return Create(newAmount, Currency);
    }

    // Métodos de comparación

    public bool IsGreaterThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount < other.Amount;
    }

    public bool IsZero() => Amount == 0;

    public bool IsPositive() => Amount > 0;

    public bool IsNegative() => Amount < 0;

    // Método auxiliar para validar monedas
    private void ValidateSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {Currency} and {other.Currency}");
    }

    // Formateo para presentación

    /// <summary>
    /// Formatea el dinero para mostrar al usuario
    /// </summary>
    public string Format(string? culture = null)
    {
        return culture?.ToLowerInvariant() switch
        {
            "us" => $"{Currency} {Amount:N2}",
            "es" => $"{Amount:N2} {Currency}",
            "fr" => $"{Amount:N2} {Currency}",
            _ => $"{Currency} {Amount:N2}"
        };
    }

    /// <summary>
    /// Representación simplificada (para debugging)
    /// </summary>
    public override string ToString() => $"{Currency} {Amount:N2}";

    // Implementación de Value Object (igualdad por valor)

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    // Operadores sobrecargados para sintaxis más natural

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public static Result<Money> operator +(Money left, Money right) => left.Add(right);
    public static Result<Money> operator -(Money left, Money right) => left.Subtract(right);
    public static Result<Money> operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Result<Money> operator /(Money money, decimal divisor) => money.Divide(divisor);
}