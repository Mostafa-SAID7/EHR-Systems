namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when a token is invalid or malformed
/// </summary>
public class InvalidTokenException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the InvalidTokenException class
    /// </summary>
    /// <param name="message">The error message describing why the token is invalid</param>
    public InvalidTokenException(string message) 
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the InvalidTokenException class with default message
    /// </summary>
    public InvalidTokenException() 
        : base("The provided token is invalid or malformed.")
    {
    }
}
