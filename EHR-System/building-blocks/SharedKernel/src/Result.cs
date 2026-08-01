using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.SharedKernel;

/// <summary>
/// Result pattern for functional error handling (instead of exceptions).
/// Provides either success with a value or failure with error message.
/// </summary>
public class Result
{
    /// <summary>
    /// Whether result represents success.
    /// </summary>
    public bool IsSuccess { get; protected set; }

    /// <summary>
    /// Error message (null if successful).
    /// </summary>
    public string? Error { get; protected set; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Create success result.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Create failure result.
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, error ?? "Unknown error");
    }

    /// <summary>
    /// Combine multiple results - fails if any result fails.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        if (failedResults.Count == 0)
            return Success();

        var errorMessage = string.Join("; ", failedResults.Select(r => r.Error));
        return Failure(errorMessage);
    }
}

/// <summary>
/// Result with typed value.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// The successful value (null if failed).
    /// </summary>
    public T? Value { get; }

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

/// <summary>
/// Extension methods for Result pattern.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Convert IEnumerable of Results to single Result - fails if any fails.
    /// </summary>
    public static Result Combine(this IEnumerable<Result> results)
    {
        var resultList = results.ToList();
        var failedResults = resultList.Where(r => !r.IsSuccess).ToList();

        if (failedResults.Count == 0)
            return Result.Success();

        var errorMessage = string.Join("; ", failedResults.Select(r => r.Error));
        return Result.Failure(errorMessage);
    }
}
