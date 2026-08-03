namespace Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user cannot be found
/// </summary>
public class UserNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the UserNotFoundException class
    /// </summary>
    /// <param name="userId">The ID of the user that was not found</param>
    public UserNotFoundException(Guid userId) 
        : base($"User with ID '{userId}' was not found.")
    {
        UserId = userId;
    }

    /// <summary>
    /// Initializes a new instance of the UserNotFoundException class by email
    /// </summary>
    /// <param name="email">The email of the user that was not found</param>
    public UserNotFoundException(string email) 
        : base($"User with email '{email}' was not found.")
    {
        Email = email;
    }

    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the user email
    /// </summary>
    public string? Email { get; }
}
