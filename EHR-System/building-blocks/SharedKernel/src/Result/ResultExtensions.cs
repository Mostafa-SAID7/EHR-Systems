using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.SharedKernel.Result;

/// <summary>
/// Extension methods for Result pattern - functional combinators.
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

    /// <summary>
    /// Pattern match on Result - execute function based on success/failure.
    /// </summary>
    public static TResult Match<TResult>(this Result result, 
        Func<TResult> onSuccess, 
        Func<string, TResult> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess() 
            : onFailure(result.Error ?? "Unknown error");
    }

    /// <summary>
    /// Pattern match on Result<T> - execute function based on success/failure.
    /// </summary>
    public static TResult Match<T, TResult>(this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return result.IsSuccess && result.Value is not null
            ? onSuccess(result.Value)
            : onFailure(result.Error ?? "Unknown error");
    }

    /// <summary>
    /// Bind - chain operations that return Result (flatMap).
    /// </summary>
    public static Result<TNew> Bind<T, TNew>(this Result<T> result,
        Func<T, Result<TNew>> binder)
    {
        if (!result.IsSuccess || result.Value is null)
            return Result<TNew>.Failure(result.Error!);

        return binder(result.Value);
    }

    /// <summary>
    /// Recover - recover from failure with fallback value.
    /// </summary>
    public static Result<T> Recover<T>(this Result<T> result,
        Func<string, T> handler)
    {
        if (result.IsSuccess)
            return result;

        try
        {
            var recoveredValue = handler(result.Error!);
            return Result<T>.Success(recoveredValue);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Recovery failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Fold - reduce to single value using success/failure handlers.
    /// </summary>
    public static T Fold<T>(this Result<T> result,
        Func<T, T> onSuccess,
        Func<string, T> onFailure)
        where T : class
    {
        if (result.IsSuccess && result.Value is not null)
            return onSuccess(result.Value);

        return onFailure(result.Error ?? "Unknown error");
    }
}
