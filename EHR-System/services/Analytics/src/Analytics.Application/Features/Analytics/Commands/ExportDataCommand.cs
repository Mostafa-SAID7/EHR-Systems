namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Export analytics data to file format
/// </summary>
public record ExportDataCommand(
    DateTime FromDate,
    DateTime ToDate,
    string Format = "CSV",
    string? Filters = null) : IRequest<ExportDataResponse>;

/// <summary>
/// Response from exporting data
/// </summary>
public record ExportDataResponse(
    bool Success,
    string Message,
    byte[]? FileContent = null,
    string? FileName = null,
    Guid? ExportId = null);
