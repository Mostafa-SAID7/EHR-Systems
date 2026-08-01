using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.DomainEvents;

public class UserUnlockedEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    public UserUnlockedEvent() { }

    public UserUnlockedEvent(Guid id, string email)
    {
        UserId = id;
        Email  = email;
    }
}


