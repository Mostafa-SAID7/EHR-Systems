using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Validation;

/// <summary>
/// Interface for object validation.
/// Single responsibility: Validation contract.
/// </summary>
public interface IValidator<T>
{
    /// <summary>
    /// Validate object synchronously.
    /// </summary>
    ValidationResult Validate(T obj);

    /// <summary>
    /// Validate object asynchronously.
    /// </summary>
    Task<ValidationResult> ValidateAsync(T obj, CancellationToken cancellationToken = default);
}
