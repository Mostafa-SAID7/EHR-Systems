namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when a token has expired
/// </summary>
public class TokenExpiredException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the TokenExpiredException class
    /// </summary>
    /// <param name="expirationTime">The time when the token expired</param>
    public TokenExpiredException(DateTime expirationTime) 
        : base($"The token expired at {expirationTime:O}.")
    {
        ExpirationTime = expirationTime;
    }

    /// <summary>
    /// Initializes a new instance of the TokenExpiredException class with default message
    /// </summary>
    public TokenExpiredException() 
        : base("The provided token has expired.")
    {
    }

    /// <summary>
    /// Gets the expiration time of the token
    /// </summary>
    public DateTime? ExpirationTime { get; }
}
