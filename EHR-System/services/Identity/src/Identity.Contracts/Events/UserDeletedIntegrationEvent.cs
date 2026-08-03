namespace Identity.Contracts.Events;

/// <summary>
/// Integration event published when a user is deleted
/// </summary>
public sealed record UserDeletedIntegrationEvent(
    Guid UserId,
    string Email,
    DateTime DeletedAt) : IntegrationEvent
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public UserDeletedIntegrationEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
        EventId = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}
