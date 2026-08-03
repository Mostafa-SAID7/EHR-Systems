namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when a password does not meet requirements
/// </summary>
public class InvalidPasswordException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the InvalidPasswordException class
    /// </summary>
    /// <param name="message">The error message describing why the password is invalid</param>
    public InvalidPasswordException(string message) 
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the InvalidPasswordException class with default message
    /// </summary>
    public InvalidPasswordException() 
        : base("Password does not meet security requirements. Must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.")
    {
    }
}
