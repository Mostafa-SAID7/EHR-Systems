using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// Access log (who accessed what and when).
/// </summary>
public class AccessLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public DateTime AccessedAt { get; set; }
    public int DurationSeconds { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public bool IsExport { get; set; }
    public bool IsPrint { get; set; }
}

