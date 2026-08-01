namespace EHRPlatform.Services.Audit.Application.Features.Audit.Queries;

using MediatR;

/// <summary>
/// Query to get audit activity for a specific user.
/// Cached 1 hour.
/// </summary>
public class GetUserAuditActivityQuery : IRequest<GetUserAuditActivityResponse>
{
    public Guid UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetUserAuditActivityResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<AuditEntryDto> Entries { get; set; } = new();
    public int TotalCount { get; set; }
}
