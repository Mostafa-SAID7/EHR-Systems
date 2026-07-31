#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a HIPAA compliance rule is violated.
/// Single responsibility: Represent HIPAA violations only.
/// </summary>
public class HIPAAException : DomainException
{
    /// <summary>
    /// Error code for HIPAA violations.
    /// </summary>
    public override string ErrorCode => ErrorCode.HIPAAViolation;

    /// <summary>
    /// Initialize with HIPAA violation message.
    /// </summary>
    public HIPAAException(string message)
        : base($"HIPAA violation: {message}", ErrorCode.HIPAAViolation)
    {
    }
}
