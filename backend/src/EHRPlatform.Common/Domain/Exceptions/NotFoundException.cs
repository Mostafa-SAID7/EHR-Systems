#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a resource is not found.
/// Single responsibility: Represent resource not found errors only.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Error code for not found errors.
    /// </summary>
    public override string ErrorCode => ErrorCode.NotFound;

    /// <summary>
    /// Initialize with resource name and ID.
    /// </summary>
    public NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with ID '{id}' was not found.", ErrorCode.NotFound)
    {
    }

    /// <summary>
    /// Initialize with custom message.
    /// </summary>
    public NotFoundException(string message)
        : base(message, ErrorCode.NotFound)
    {
    }
}
