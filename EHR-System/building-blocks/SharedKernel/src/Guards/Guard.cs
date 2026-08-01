using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.SharedKernel.Guards;

/// <summary>
/// Guard clauses for input validation and preconditions.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throw if value is null.
    /// </summary>
    public static void AgainstNull<T>(T? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
    }

    /// <summary>
    /// Throw if string is null or empty.
    /// </summary>
    public static void AgainstNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throw if collection is null or empty.
    /// </summary>
    public static void AgainstNullOrEmpty<T>(IEnumerable<T>? collection, string paramName)
    {
        if (collection?.Any() != true)
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throw if number is negative.
    /// </summary>
    public static void AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentException($"{paramName} cannot be negative", paramName);
    }

    /// <summary>
    /// Throw if number is negative or zero.
    /// </summary>
    public static void AgainstNegativeOrZero(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{paramName} must be greater than zero", paramName);
    }

    /// <summary>
    /// Throw if value is outside range.
    /// </summary>
    public static void AgainstOutOfRange<T>(T value, T minValue, T maxValue, string paramName)
        where T : IComparable<T>
    {
        if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
            throw new ArgumentOutOfRangeException(
                paramName, 
                $"{paramName} must be between {minValue} and {maxValue}");
    }

    /// <summary>
    /// Throw if condition is true.
    /// </summary>
    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new InvalidOperationException(message);
    }
}
