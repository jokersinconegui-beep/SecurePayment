namespace Domain.Common;

/// <summary>
/// Representa el resultado de una operación que puede fallar
/// Patrón usado para evitar excepciones en lógica de dominio
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error");
        
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error");
        
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

/// <summary>
/// Representa el resultado de una operación que devuelve un valor T
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;
    
    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException($"Cannot access value of failed result: {Error}");
            return _value!;
        }
    }
    
    private Result(T value) : base(true, string.Empty)
    {
        _value = value;
    }
    
    private Result(string error) : base(false, error)
    {
        _value = default;
    }
    
    public static Result<T> Success(T value) => new Result<T>(value);
    public static new Result<T> Failure(string error) => new Result<T>(error);
    
    // Operadores implícitos para facilitar uso
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);
}