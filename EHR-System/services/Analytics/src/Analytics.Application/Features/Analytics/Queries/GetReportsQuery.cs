namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;

/// <summary>
/// Query to get all reports
/// </summary>
public record GetReportsQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetReportsResponse>;

/// <summary>
/// Response with list of reports
/// </summary>
public record GetReportsResponse(
    bool Success,
    string? Message,
    List<ReportListItemDto> Reports,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record ReportListItemDto(
    Guid Id,
    string Name,
    string Description,
    string ReportType,
    bool IsScheduled,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int ExecutionCount);
