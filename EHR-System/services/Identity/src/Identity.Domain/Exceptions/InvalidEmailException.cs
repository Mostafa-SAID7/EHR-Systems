namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when an email address is invalid
/// </summary>
public class InvalidEmailException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the InvalidEmailException class
    /// </summary>
    /// <param name="email">The invalid email address</param>
    public InvalidEmailException(string email) 
        : base($"The email address '{email}' is invalid.")
    {
        Email = email;
    }

    /// <summary>
    /// Gets the invalid email address
    /// </summary>
    public string Email { get; }
}
