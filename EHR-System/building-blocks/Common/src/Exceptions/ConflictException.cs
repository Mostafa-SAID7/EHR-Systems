using System;

namespace EHRPlatform.Common.Exceptions;

/// <summary>
/// Conflict error exception (409).
/// Single responsibility: Conflict error handling.
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message, string? resourceType = null)
        : base(message, "CONFLICT", isUserFacingError: true,
            details: resourceType != null ? new { ResourceType = resourceType } : null)
    {
    }
}
