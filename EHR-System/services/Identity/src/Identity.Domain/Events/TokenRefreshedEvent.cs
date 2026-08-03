namespace Identity.Domain.Events;

/// <summary>
/// Domain event raised when a token is refreshed
/// </summary>
public sealed record TokenRefreshedEvent(
    Guid UserId,
    Guid TokenId,
    DateTime RefreshedAt) : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the TokenRefreshedEvent class
    /// </summary>
    public TokenRefreshedEvent() 
        : this(Guid.Empty, Guid.Empty, DateTime.UtcNow)
    {
    }
}
