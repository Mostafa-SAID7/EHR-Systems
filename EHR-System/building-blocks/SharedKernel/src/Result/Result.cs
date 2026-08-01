using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.SharedKernel.Result;

/// <summary>
/// Result pattern for functional error handling (instead of exceptions).
/// Represents success or failure without value.
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

    /// <summary>
    /// Create result instance.
    /// </summary>
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
