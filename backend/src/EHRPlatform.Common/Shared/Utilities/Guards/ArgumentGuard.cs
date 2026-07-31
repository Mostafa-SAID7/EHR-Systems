namespace EHRPlatform.Common.Shared.Utilities.Guards;

/// <summary>
/// Lightweight argument validation helpers used throughout the EHR Platform.
/// Centralises null/empty/range checks and avoids duplication across files.
/// </summary>
public static class ArgumentGuard
{
    // ── Null / empty ──────────────────────────────────────────────────────────

    public static T NotNull<T>(T? argument, string parameterName) where T : class
    {
        if (argument is null)
            throw new ArgumentNullException(parameterName);
        return argument;
    }

    public static string NotNullOrEmpty(string? argument, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("Value cannot be null or empty.", parameterName);
        return argument;
    }

    // ── Guid ─────────────────────────────────────────────────────────────────

    public static Guid NotDefault(Guid argument, string parameterName)
    {
        if (argument == Guid.Empty)
            throw new ArgumentException("Value must not be an empty GUID.", parameterName);
        return argument;
    }

    // ── Numeric ranges ───────────────────────────────────────────────────────

    public static int IsPositive(int argument, string parameterName)
    {
        if (argument <= 0)
            throw new ArgumentOutOfRangeException(parameterName, argument, "Value must be greater than zero.");
        return argument;
    }

    public static long IsPositive(long argument, string parameterName)
    {
        if (argument <= 0)
            throw new ArgumentOutOfRangeException(parameterName, argument, "Value must be greater than zero.");
        return argument;
    }

    public static int IsInRange(int argument, int min, int max, string parameterName)
    {
        if (argument < min || argument > max)
            throw new ArgumentOutOfRangeException(parameterName, argument,
                $"Value must be between {min} and {max} inclusive.");
        return argument;
    }

    public static double IsInRange(double argument, double min, double max, string parameterName)
    {
        if (argument < min || argument > max)
            throw new ArgumentOutOfRangeException(parameterName, argument,
                $"Value must be between {min} and {max} inclusive.");
        return argument;
    }

    public static int NotNegative(int argument, string parameterName)
    {
        if (argument < 0)
            throw new ArgumentOutOfRangeException(parameterName, argument, "Value must not be negative.");
        return argument;
    }

    // ── Collections ──────────────────────────────────────────────────────────

    public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? argument, string parameterName)
    {
        if (argument is null || !argument.Any())
            throw new ArgumentException("Collection must not be null or empty.", parameterName);
        return argument;
    }

    // ── String length ─────────────────────────────────────────────────────────

    public static string MaxLength(string argument, int maxLength, string parameterName)
    {
        NotNullOrEmpty(argument, parameterName);
        if (argument.Length > maxLength)
            throw new ArgumentException(
                $"Value exceeds maximum length of {maxLength} characters.", parameterName);
        return argument;
    }

    // ── Enum ─────────────────────────────────────────────────────────────────

    public static T IsDefined<T>(T argument, string parameterName) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), argument))
            throw new ArgumentException(
                $"Value '{argument}' is not a valid {typeof(T).Name}.", parameterName);
        return argument;
    }
}

