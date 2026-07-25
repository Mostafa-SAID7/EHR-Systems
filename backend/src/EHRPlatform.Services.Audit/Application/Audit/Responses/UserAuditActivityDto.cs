namespace EHRPlatform.Services.Audit.Application.Audit.Responses;

/// <summary>
/// Paginated audit activity for a specific user.
/// </summary>
public class UserAuditActivityDto
{
    public Guid UserId { get; set; }
    public List<AuditEntryResponse> Activities { get; set; } = new();
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}
