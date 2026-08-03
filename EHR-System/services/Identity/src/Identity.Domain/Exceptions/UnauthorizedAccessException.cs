namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when access is denied due to insufficient permissions
/// </summary>
public class UnauthorizedAccessException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the UnauthorizedAccessException class
    /// </summary>
    /// <param name="userId">The ID of the user attempting unauthorized access</param>
    /// <param name="resource">The resource being accessed</param>
    public UnauthorizedAccessException(Guid userId, string resource) 
        : base($"User '{userId}' does not have permission to access '{resource}'.")
    {
        UserId = userId;
        Resource = resource;
    }

    /// <summary>
    /// Initializes a new instance of the UnauthorizedAccessException class with custom message
    /// </summary>
    /// <param name="message">The error message</param>
    public UnauthorizedAccessException(string message) 
        : base(message)
    {
    }

    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the resource being accessed
    /// </summary>
    public string? Resource { get; }
}
