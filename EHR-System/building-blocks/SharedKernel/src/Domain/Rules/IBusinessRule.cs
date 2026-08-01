namespace EHRPlatform.SharedKernel.Domain.Rules;

/// <summary>
/// Business rule contract.
/// Implements specification pattern for business logic.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Check if business rule is broken.
    /// </summary>
    bool IsBroken();

    /// <summary>
    /// Error message if rule is broken.
    /// </summary>
    string Message { get; }
}
