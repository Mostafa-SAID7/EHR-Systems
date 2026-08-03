namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a user that already exists
/// </summary>
public class UserAlreadyExistsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the UserAlreadyExistsException class
    /// </summary>
    /// <param name="email">The email of the existing user</param>
    public UserAlreadyExistsException(string email) 
        : base($"A user with email '{email}' already exists.")
    {
        Email = email;
    }

    /// <summary>
    /// Gets the email of the existing user
    /// </summary>
    public string Email { get; }
}
