namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to update report
/// </summary>
public record UpdateReportCommand(
    Guid ReportId,
    string? Name = null,
    string? Description = null,
    string? Configuration = null,
    bool? IsScheduled = null,
    string? ScheduleCron = null) : IRequest<UpdateReportResponse>;

/// <summary>
/// Response from updating report
/// </summary>
public record UpdateReportResponse(
    bool Success,
    string Message);
