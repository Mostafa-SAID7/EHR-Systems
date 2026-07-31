#nullable enable

using FluentValidation;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Application.Common.Validators;

/// <summary>
/// Common validation rules used across all services.
/// Inject as needed into service-specific validators.
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Email validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");
    }

    /// <summary>
    /// Phone number validation rule (US format).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPhoneNumber<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .Matches(@"^\+?1?\d{9,15}$")
            .WithMessage("Phone number must be between 9-15 digits");
    }

    /// <summary>
    /// Name validation rule (first/last name).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidName<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes");
    }

    /// <summary>
    /// Password validation rule (strong password).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }

    /// <summary>
    /// URL validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidUrl<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("URL is required")
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("URL must be a valid absolute URL");
    }

    /// <summary>
    /// Medical Record Number (MRN) validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidMRN<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("MRN is required")
            .Matches(@"^[A-Z0-9\-]{5,20}$")
            .WithMessage("MRN must be 5-20 characters and contain only letters, numbers, and hyphens");
    }

    /// <summary>
    /// ICD-10 code validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidICD10Code<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("ICD-10 code is required")
            .Matches(@"^[A-Z]\d[A-Z0-9](\.\d{1,2})?$")
            .WithMessage("ICD-10 code format is invalid (e.g., A00 or A00.0)");
    }

    /// <summary>
    /// CPT code validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCPTCode<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("CPT code is required")
            .Matches(@"^\d{5}([A-Z]{0,2})?$")
            .WithMessage("CPT code must be 5 digits, optionally followed by 0-2 letters");
    }

    /// <summary>
    /// NPI (National Provider Identifier) validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidNPI<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("NPI is required")
            .Matches(@"^\d{10}$")
            .WithMessage("NPI must be exactly 10 digits");
    }

    /// <summary>
    /// Insurance policy number validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPolicyNumber<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty().WithMessage("Policy number is required")
            .MinimumLength(5).WithMessage("Policy number must be at least 5 characters")
            .MaximumLength(50).WithMessage("Policy number cannot exceed 50 characters");
    }

    /// <summary>
    /// Positive amount validation rule (for prices, amounts).
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidPositiveAmount<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .ScalePrecision(2, 10)
            .WithMessage("Amount can have maximum 2 decimal places");
    }

    /// <summary>
    /// Non-negative amount validation rule (for quantities, balances).
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidNonNegativeAmount<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0")
            .ScalePrecision(2, 10)
            .WithMessage("Amount can have maximum 2 decimal places");
    }

    /// <summary>
    /// Date of birth validation rule (must be in the past).
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> ValidDateOfBirth<T>(this IRuleBuilder<T, DateTime> rule)
    {
        return rule
            .LessThan(DateTimeHelper.UtcNow)
            .WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTimeHelper.UtcNow.AddYears(-150))
            .WithMessage("Date of birth is unrealistic (more than 150 years ago)");
    }

    /// <summary>
    /// Future date validation rule (for appointments, etc.).
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> ValidFutureDate<T>(this IRuleBuilder<T, DateTime> rule)
    {
        return rule
            .GreaterThan(DateTimeHelper.UtcNow)
            .WithMessage("Date must be in the future");
    }

    /// <summary>
    /// Appointment start time cannot be before end time.
    /// </summary>
    public static IRuleBuilderOptions<T, (DateTime Start, DateTime End)> ValidDateRange<T>(
        this IRuleBuilder<T, (DateTime Start, DateTime End)> rule)
    {
        return rule
            .Must(x => x.Start < x.End)
            .WithMessage("Start time must be before end time");
    }

    /// <summary>
    /// Valid blood type validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidBloodType<T>(this IRuleBuilder<T, string?> rule)
    {
        var validTypes = new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" };
        return rule
            .NotEmpty().WithMessage("Blood type is required")
            .Must(x => validTypes.Contains(x))
            .WithMessage("Blood type must be one of: O+, O-, A+, A-, B+, B-, AB+, AB-");
    }

    /// <summary>
    /// Valid gender validation rule.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidGender<T>(this IRuleBuilder<T, string?> rule)
    {
        var validGenders = new[] { "Male", "Female", "Other", "Prefer" };
        return rule
            .NotEmpty().WithMessage("Gender is required")
            .Must(x => validGenders.Contains(x))
            .WithMessage("Gender must be one of: Male, Female, Other, Prefer");
    }
}

