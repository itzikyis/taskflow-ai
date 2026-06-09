namespace TaskFlow.Domain.Common;

/// <summary>Represents the outcome of an operation that may succeed or fail.</summary>
public sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Error = Error.None;
    }

    private Result(Error error)
    {
        Value = default;
        IsSuccess = false;
        Error = error;
    }

    /// <summary>Gets the value when the result is successful.</summary>
    public T? Value { get; }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error when the result is a failure.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result.</summary>
    public static Result<T> Failure(Error error) => new(error);
}

/// <summary>Non-generic result for operations that return no value.</summary>
public sealed class Result
{
    private Result() { IsSuccess = true; Error = Error.None; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error when the result is a failure.</summary>
    public Error Error { get; }

    /// <summary>A successful result with no value.</summary>
    public static readonly Result Ok = new();

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(error);
}
