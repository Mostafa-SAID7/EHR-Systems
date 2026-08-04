namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Base domain exception for all domain-level errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets error code for logging and client notifications
    /// </summary>
    public virtual string ErrorCode => GetType().Name;
}
