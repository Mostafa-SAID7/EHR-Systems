using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// Data change audit (before/after tracking).
/// </summary>
public class DataChangeAudit : BaseEntity
{
    public Guid UserId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public DateTime ChangedAt { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Modified, Deleted
    public string? Reason { get; set; }
}

