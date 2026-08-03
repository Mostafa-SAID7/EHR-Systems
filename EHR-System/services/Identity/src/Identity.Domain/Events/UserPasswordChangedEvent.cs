namespace Identity.Domain.Events;

/// <summary>
/// Domain event raised when a user password is changed
/// </summary>
public sealed record UserPasswordChangedEvent(
    Guid UserId,
    string Email,
    DateTime ChangedAt) : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the UserPasswordChangedEvent class
    /// </summary>
    public UserPasswordChangedEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
    }
}
