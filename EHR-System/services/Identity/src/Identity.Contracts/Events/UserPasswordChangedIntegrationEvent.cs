namespace Identity.Contracts.Events;

/// <summary>
/// Integration event published when a user password is changed
/// </summary>
public sealed record UserPasswordChangedIntegrationEvent(
    Guid UserId,
    string Email,
    DateTime ChangedAt) : IntegrationEvent
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public UserPasswordChangedIntegrationEvent() 
        : this(Guid.Empty, string.Empty, DateTime.UtcNow)
    {
        EventId = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}
