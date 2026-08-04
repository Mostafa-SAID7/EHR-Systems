namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to create report
/// </summary>
public record CreateReportCommand(
    string Name,
    string Description,
    string ReportType,
    string? Configuration = null,
    bool IsScheduled = false,
    string? ScheduleCron = null) : IRequest<CreateReportResponse>;

/// <summary>
/// Response from creating report
/// </summary>
public record CreateReportResponse(
    bool Success,
    string Message,
    Guid? ReportId = null);
