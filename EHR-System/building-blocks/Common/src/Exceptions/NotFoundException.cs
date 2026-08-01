using System;

namespace EHRPlatform.Common.Exceptions;

/// <summary>
/// Not found error exception.
/// Single responsibility: Not found error handling.
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message, string? resourceType = null)
        : base(message, "NOT_FOUND", isUserFacingError: true,
            details: resourceType != null ? new { ResourceType = resourceType } : null)
    {
    }
}
