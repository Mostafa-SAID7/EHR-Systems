using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.Events;

public class MfaEnabledEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    public MfaEnabledEvent() { }

    public MfaEnabledEvent(Guid id, string email)
    {
        UserId = id;
        Email  = email;
    }
}

