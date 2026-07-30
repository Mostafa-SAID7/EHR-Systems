using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// Login audit trail for security monitoring.
/// </summary>
public class LoginAudit : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}

