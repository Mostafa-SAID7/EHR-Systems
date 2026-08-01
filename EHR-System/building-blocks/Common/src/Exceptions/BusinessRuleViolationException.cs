using System;

namespace EHRPlatform.Common.Exceptions;

/// <summary>
/// Business rule violation exception.
/// Single responsibility: Business rule violation error handling.
/// </summary>
public class BusinessRuleViolationException : ApplicationException
{
    public BusinessRuleViolationException(string message, string? ruleCode = null)
        : base(message, ruleCode ?? "BUSINESS_RULE_VIOLATION", isUserFacingError: true)
    {
    }
}
