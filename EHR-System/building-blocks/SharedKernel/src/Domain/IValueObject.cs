namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Interface marking a value object.
/// Single responsibility: Value object contract.
/// </summary>
public interface IValueObject
{
    /// <summary>
    /// Get atomic value for comparison.
    /// </summary>
    object GetAtomicValue();
}
