#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when an external service fails.
/// Single responsibility: Represent external service failures only.
/// </summary>
public class ExternalServiceException : DomainException
{
    /// <summary>
    /// Error code for external service errors.
    /// </summary>
    public override string ErrorCode => ErrorCode.ExternalServiceError;

    /// <summary>
    /// Initialize with service name, error message, and optional inner exception.
    /// </summary>
    public ExternalServiceException(string serviceName, string message, Exception? innerException = null)
        : base($"External service '{serviceName}' error: {message}", ErrorCode.ExternalServiceError, innerException)
    {
    }
}
