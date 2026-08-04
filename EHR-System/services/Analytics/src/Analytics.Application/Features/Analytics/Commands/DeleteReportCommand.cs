namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to delete report
/// </summary>
public record DeleteReportCommand(
    Guid ReportId) : IRequest<DeleteReportResponse>;

/// <summary>
/// Response from deleting report
/// </summary>
public record DeleteReportResponse(
    bool Success,
    string Message);
