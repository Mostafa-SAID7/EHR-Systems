using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.Events;

public class RoleAssignedEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public RoleAssignedEvent() { }

    public RoleAssignedEvent(Guid userId, Guid roleId, string roleName)
    {
        UserId   = userId;
        RoleId   = roleId;
        RoleName = roleName;
    }
}

