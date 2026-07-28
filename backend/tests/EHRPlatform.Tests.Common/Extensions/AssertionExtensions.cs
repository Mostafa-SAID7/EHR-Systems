#nullable enable

using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace EHRPlatform.Tests.Common.Extensions;

/// <summary>
/// Custom assertion extensions for common test validations.
/// </summary>
public static class AssertionExtensions
{
    /// <summary>
    /// Assert value is a valid email address.
    /// </summary>
    public static AndConstraint<StringAssertions> BeValidEmail(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        var subject = assertions.Subject;

        if (!Regex.IsMatch(subject, emailPattern))
        {
            Execute.Assertion
                .ForCondition(false)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be a valid email address, but found {0}", subject);
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert value is a valid phone number.
    /// </summary>
    public static AndConstraint<StringAssertions> BeValidPhoneNumber(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        const string phonePattern = @"^\+?1?\d{9,15}$";
        var subject = assertions.Subject;

        if (!Regex.IsMatch(subject.Replace("-", "").Replace(" ", ""), phonePattern))
        {
            Execute.Assertion
                .ForCondition(false)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be a valid phone number, but found {0}", subject);
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert value is a valid Medical Record Number (MRN).
    /// </summary>
    public static AndConstraint<StringAssertions> BeValidMRN(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        // Typical format: 999999-999
        const string mrnPattern = @"^\d{6}-\d{3}$";
        var subject = assertions.Subject;

        if (!Regex.IsMatch(subject, mrnPattern))
        {
            Execute.Assertion
                .ForCondition(false)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be a valid MRN, but found {0}", subject);
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert value is a valid Social Security Number (SSN).
    /// </summary>
    public static AndConstraint<StringAssertions> BeValidSSN(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        const string ssnPattern = @"^\d{3}-\d{2}-\d{4}$";
        var subject = assertions.Subject;

        if (!Regex.IsMatch(subject, ssnPattern))
        {
            Execute.Assertion
                .ForCondition(false)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be a valid SSN, but found {0}", subject);
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert value completes within specified milliseconds.
    /// </summary>
    public static void CompleteWithinMs(this Action action, int milliseconds)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > milliseconds)
        {
            Execute.Assertion
                .ForCondition(false)
                .FailWith($"Expected action to complete within {milliseconds}ms, but took {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Assert async operation completes within specified milliseconds.
    /// </summary>
    public static async System.Threading.Tasks.Task CompleteWithinMsAsync(
        this Func<System.Threading.Tasks.Task> asyncAction, int milliseconds)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await asyncAction();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > milliseconds)
        {
            Execute.Assertion
                .ForCondition(false)
                .FailWith($"Expected async action to complete within {milliseconds}ms, but took {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Assert string contains no SQL injection attempts.
    /// </summary>
    public static AndConstraint<StringAssertions> NotContainSqlInjection(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        var sqlKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "INSERT", "UPDATE", "EXEC", "EXECUTE", "UNION" };
        var subject = assertions.Subject?.ToUpperInvariant() ?? "";

        foreach (var keyword in sqlKeywords)
        {
            if (subject.Contains(keyword))
            {
                Execute.Assertion
                    .ForCondition(false)
                    .BecauseOf(because, becauseArgs)
                    .FailWith($"Expected {{context:string}} to not contain SQL keyword '{keyword}', but found {{0}}", subject);
            }
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert string does not contain XSS attempt.
    /// </summary>
    public static AndConstraint<StringAssertions> NotContainXss(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        var xssPatterns = new[] { "<script", "javascript:", "onerror=", "onload=", "<iframe" };
        var subject = assertions.Subject?.ToLowerInvariant() ?? "";

        foreach (var pattern in xssPatterns)
        {
            if (subject.Contains(pattern))
            {
                Execute.Assertion
                    .ForCondition(false)
                    .BecauseOf(because, becauseArgs)
                    .FailWith($"Expected {{context:string}} to not contain XSS pattern '{pattern}', but found {{0}}", subject);
            }
        }

        return new AndConstraint<StringAssertions>(assertions);
    }

    /// <summary>
    /// Assert string is ISO 8601 datetime format.
    /// </summary>
    public static AndConstraint<StringAssertions> BeValidIso8601DateTime(
        this StringAssertions assertions, string because = "", params object[] becauseArgs)
    {
        var subject = assertions.Subject;

        if (!DateTime.TryParse(subject, System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.RoundtripKind, out _))
        {
            Execute.Assertion
                .ForCondition(false)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be valid ISO 8601 datetime, but found {0}", subject);
        }

        return new AndConstraint<StringAssertions>(assertions);
    }
}
