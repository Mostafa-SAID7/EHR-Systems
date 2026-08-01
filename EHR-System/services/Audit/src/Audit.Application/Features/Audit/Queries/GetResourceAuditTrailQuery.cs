namespace EHRPlatform.Services.Audit.Application.Features.Audit.Queries;

using MediatR;

/// <summary>
/// Query to get audit trail for a specific resource.
/// Cached 1 hour.
/// </summary>
public class GetResourceAuditTrailQuery : IRequest<GetResourceAuditTrailResponse>
{
    public Guid ResourceId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetResourceAuditTrailResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<AuditEntryDto> Entries { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class AuditEntryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string? ChangeDetails { get; set; }
    public bool ContainsSsn { get; set; }
    public bool ContainsDob { get; set; }
    public bool ContainsMrn { get; set; }
    public DateTime CreatedAt { get; set; }
}
