namespace Identity.Domain.Events;

/// <summary>
/// Domain event raised when a user logs in
/// </summary>
public sealed record UserLoggedInEvent(
    Guid UserId,
    string Email,
    DateTime LoggedInAt,
    string? IpAddress = null,
    string? UserAgent = null) : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the UserLoggedInEvent class
    /// </summary>
    public UserLoggedInEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
    }
}
