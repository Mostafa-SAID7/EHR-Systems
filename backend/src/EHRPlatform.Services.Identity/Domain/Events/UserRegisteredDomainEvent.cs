using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Identity.Domain.Events;

/// <summary>
/// In-process domain event raised when a new user self-registers.
/// Consumed by pipeline behaviors within the Identity service.
/// For cross-service messaging, use <see cref="UserCreatedEvent"/> (IntegrationEvent).
/// </summary>
public class UserRegisteredDomainEvent : DomainEvent
{
    public Guid   UserId    { get; set; }
    public string Email     { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
}


