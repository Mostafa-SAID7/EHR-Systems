using System;

namespace EHRPlatform.SharedKernel.Result;

/// <summary>
/// Result with typed value - represents success with value or failure.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// The successful value (null if failed).
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Create result instance.
    /// </summary>
    private Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Create success result with value.
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Create failure result.
    /// </summary>
    public new static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error ?? "Unknown error");
    }

    /// <summary>
    /// Map result to another type (if successful).
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (!IsSuccess || Value is null)
            return Result<TNew>.Failure(Error!);

        return Result<TNew>.Success(mapper(Value));
    }

    /// <summary>
    /// Flat map result (for chaining operations that return Result).
    /// </summary>
    public Result<TNew> FlatMap<TNew>(Func<T, Result<TNew>> mapper)
    {
        if (!IsSuccess || Value is null)
            return Result<TNew>.Failure(Error!);

        return mapper(Value);
    }

    /// <summary>
    /// Execute action if successful.
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess && Value is not null)
            action(Value);

        return this;
    }

    /// <summary>
    /// Get value or throw exception.
    /// </summary>
    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            throw new InvalidOperationException(Error);

        return Value!;
    }

    /// <summary>
    /// Get value or default.
    /// </summary>
    public T? GetValueOrDefault(T? defaultValue = default)
    {
        return IsSuccess ? Value : defaultValue;
    }
}
