namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;

/// <summary>
/// Get metrics for date range with optional filters
/// </summary>
public record GetMetricsQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? MetricType = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetMetricsResponse>;

/// <summary>
/// Response with metrics
/// </summary>
public record GetMetricsResponse(
    bool Success,
    string Message,
    IEnumerable<MetricDataDto> Metrics,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Metric data point
/// </summary>
public record MetricDataDto(
    string MetricName,
    decimal Value,
    DateTime Timestamp,
    string Unit);
