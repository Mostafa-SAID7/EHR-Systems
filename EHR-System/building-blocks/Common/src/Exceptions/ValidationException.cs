using System;
using System.Collections.Generic;

namespace EHRPlatform.Common.Exceptions;

/// <summary>
/// Validation error exception.
/// Single responsibility: Validation error handling.
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException(string message, Dictionary<string, object>? details = null)
        : base(message, "VALIDATION_ERROR", isUserFacingError: true, details: details)
    {
    }
}
