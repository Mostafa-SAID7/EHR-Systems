using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.DomainEvents;

public class UserCreatedEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public UserCreatedEvent() { }

    public UserCreatedEvent(Guid id, string email, string firstName, string lastName)
    {
        UserId    = id;
        Email     = email;
        FirstName = firstName;
        LastName  = lastName;
    }
}


