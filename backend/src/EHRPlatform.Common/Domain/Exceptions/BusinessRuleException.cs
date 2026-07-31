#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// Single responsibility: Represent business rule violations only.
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// Error code for business rule violations.
    /// </summary>
    public override string ErrorCode => ErrorCode.BusinessRuleViolation;

    /// <summary>
    /// Initialize with business rule violation message.
    /// </summary>
    public BusinessRuleException(string message)
        : base(message, ErrorCode.BusinessRuleViolation)
    {
    }
}
