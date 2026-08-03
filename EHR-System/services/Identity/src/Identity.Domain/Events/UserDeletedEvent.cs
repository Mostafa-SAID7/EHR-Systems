namespace Identity.Domain.Events;

/// <summary>
/// Domain event raised when a user is deleted
/// </summary>
public sealed record UserDeletedEvent(
    Guid UserId,
    string Email,
    DateTime DeletedAt) : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the UserDeletedEvent class
    /// </summary>
    public UserDeletedEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
    }
}
