namespace Identity.Domain.Events;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public sealed record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt) : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the UserCreatedEvent class
    /// </summary>
    public UserCreatedEvent() 
        : this(Guid.Empty, string.Empty, string.Empty, string.Empty, DateTime.UtcNow)
    {
    }
}
