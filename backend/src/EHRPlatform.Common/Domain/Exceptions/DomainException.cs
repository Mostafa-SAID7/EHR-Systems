#nullable enable

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Gets the error code for the exception.
    /// </summary>
    public virtual string ErrorCode { get; }

    /// <summary>
    /// Gets the correlation ID for tracking.
    /// </summary>
    public string? CorrelationId { get; set; }

    protected DomainException(string message, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when a validation rule is violated.
/// </summary>
public class ValidationException : DomainException
{
    public override string ErrorCode => "VALIDATION_ERROR";

    public ValidationException(string message, Exception? innerException = null)
        : base(message, "VALIDATION_ERROR", innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public override string ErrorCode => "NOT_FOUND";

    public NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with ID '{id}' was not found.", "NOT_FOUND")
    {
    }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception thrown when a HIPAA rule is violated.
/// </summary>
public class HIPAAException : DomainException
{
    public override string ErrorCode => "HIPAA_VIOLATION";

    public HIPAAException(string message)
        : base($"HIPAA violation: {message}", "HIPAA_VIOLATION")
    {
    }
}

/// <summary>
/// Exception thrown when user lacks required permissions.
/// </summary>
public class UnauthorizedException : DomainException
{
    public override string ErrorCode => "UNAUTHORIZED";

    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string userId, string requiredPermission)
        : base($"User '{userId}' does not have permission '{requiredPermission}'.", "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Exception thrown when user attempts an action without authorization.
/// </summary>
public class ForbiddenException : DomainException
{
    public override string ErrorCode => "FORBIDDEN";

    public ForbiddenException(string message)
        : base(message, "FORBIDDEN")
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public override string ErrorCode => "BUSINESS_RULE_VIOLATION";

    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }
}

/// <summary>
/// Exception thrown when a resource already exists.
/// </summary>
public class ConflictException : DomainException
{
    public override string ErrorCode => "CONFLICT";

    public ConflictException(string message)
        : base(message, "CONFLICT")
    {
    }
}

/// <summary>
/// Exception thrown when an external service fails.
/// </summary>
public class ExternalServiceException : DomainException
{
    public override string ErrorCode => "EXTERNAL_SERVICE_ERROR";

    public ExternalServiceException(string serviceName, string message, Exception? innerException = null)
        : base($"External service '{serviceName}' error: {message}", "EXTERNAL_SERVICE_ERROR", innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an operation times out.
/// </summary>
public class TimeoutException : DomainException
{
    public override string ErrorCode => "TIMEOUT";

    public TimeoutException(string message)
        : base(message, "TIMEOUT")
    {
    }
}

