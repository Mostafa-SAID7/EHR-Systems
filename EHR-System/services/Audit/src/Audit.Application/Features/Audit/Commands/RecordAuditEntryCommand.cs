namespace EHRPlatform.Services.Audit.Application.Features.Audit.Commands;

using MediatR;

/// <summary>
/// Command to record an audit entry.
/// </summary>
public class RecordAuditEntryCommand : IRequest<RecordAuditEntryResponse>
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    
    // Optional fields
    public bool ContainsSsn { get; set; }
    public bool ContainsDob { get; set; }
    public bool ContainsMrn { get; set; }
    public bool ContainsPhoneNumber { get; set; }
    public string AccessLevel { get; set; } = "Internal";
    public string? ChangeDetails { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RecordAuditEntryResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? AuditId { get; set; }
}
