using System;

namespace EHRPlatform.SharedKernel.Domain.Rules;

/// <summary>
/// Exception thrown when business rule is violated.
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// The broken business rule.
    /// </summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>
    /// Create exception from broken rule.
    /// </summary>
    public BusinessRuleException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }
}
