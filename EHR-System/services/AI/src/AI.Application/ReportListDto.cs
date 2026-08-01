namespace EHRPlatform.Services.Analytics.Application.Analytics.Responses;

/// <summary>
/// Paginated list of reports response DTO.
/// </summary>
public class ReportListDto
{
    public List<ReportResponse> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}
