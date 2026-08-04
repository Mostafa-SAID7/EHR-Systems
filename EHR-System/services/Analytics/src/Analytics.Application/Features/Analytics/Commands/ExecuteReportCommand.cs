namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to execute report
/// </summary>
public record ExecuteReportCommand(
    Guid ReportId,
    string? Parameters = null) : IRequest<ExecuteReportResponse>;

/// <summary>
/// Response from executing report
/// </summary>
public record ExecuteReportResponse(
    bool Success,
    string Message,
    Guid? ExecutionId = null,
    string? OutputLocation = null);
