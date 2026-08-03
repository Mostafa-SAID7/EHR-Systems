namespace Identity.Contracts.Events;

/// <summary>
/// Integration event published when a user is created
/// </summary>
public sealed record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt) : IntegrationEvent
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public UserCreatedIntegrationEvent() 
        : this(Guid.Empty, string.Empty, string.Empty, string.Empty, DateTime.UtcNow)
    {
        EventId = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}
