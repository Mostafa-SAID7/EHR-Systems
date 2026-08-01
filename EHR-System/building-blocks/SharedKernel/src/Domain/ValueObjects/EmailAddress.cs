using System;
using System.Text.RegularExpressions;
using EHRPlatform.SharedKernel.Result;

namespace EHRPlatform.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value object for email address with validation.
/// </summary>
public class EmailAddress : ValueObject
{
    /// <summary>
    /// Email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Create email address value object.
    /// </summary>
    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Create email address with validation.
    /// </summary>
    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<EmailAddress>.Failure("Email address is required");

        email = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(email))
            return Result<EmailAddress>.Failure("Email address format is invalid");

        return Result<EmailAddress>.Success(new EmailAddress(email));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get local part (before @).
    /// </summary>
    public string GetLocalPart()
    {
        var parts = Value.Split('@');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    /// <summary>
    /// Get domain part (after @).
    /// </summary>
    public string GetDomain()
    {
        var parts = Value.Split('@');
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    protected override System.Collections.Generic.IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
