namespace Identity.Contracts.Events;

/// <summary>
/// Integration event published when a user logs in
/// </summary>
public sealed record UserLoggedInIntegrationEvent(
    Guid UserId,
    string Email,
    DateTime LoggedInAt,
    string? IpAddress = null) : IntegrationEvent
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public UserLoggedInIntegrationEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
        EventId = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}
