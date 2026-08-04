namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for exporting analytics data
/// </summary>
public class ExportDataCommandHandler : IRequestHandler<ExportDataCommand, ExportDataResponse>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ExportDataCommandHandler> _logger;

    public ExportDataCommandHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<ExportDataCommandHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<ExportDataResponse> Handle(
        ExportDataCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting analytics data from {FromDate} to {ToDate} in {Format}", 
            command.FromDate, command.ToDate, command.Format);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new ExportDataResponse(
                    Success: false,
                    Message: "Tenant context not available");
            }

            // Query metrics within date range
            var metrics = await _metricRepository.GetByTimeRangeAsync(
                command.FromDate, command.ToDate, tenantId);

            if (!metrics.Any())
            {
                return new ExportDataResponse(
                    Success: false,
                    Message: "No metrics found for the specified date range");
            }

            // Format data based on requested format
            byte[] fileContent = command.Format.ToUpper() switch
            {
                "CSV" => GenerateCsvContent(metrics.ToList()),
                "JSON" => GenerateJsonContent(metrics.ToList()),
                _ => GenerateCsvContent(metrics.ToList())
            };

            var fileName = $"analytics_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{command.Format.ToLower()}";

            _logger.LogInformation("Data exported successfully: {FileName}", fileName);

            return new ExportDataResponse(
                Success: true,
                Message: "Data exported successfully",
                FileContent: fileContent,
                FileName: fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics data");
            return new ExportDataResponse(
                Success: false,
                Message: $"Failed to export data: {ex.Message}");
        }
    }

    private byte[] GenerateCsvContent(List<Domain.Entities.AnalyticsMetric> metrics)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Id,MetricName,Category,Value,Unit,Timestamp,Dimension1,Dimension2,Dimension3");

        foreach (var metric in metrics)
        {
            csv.AppendLine($"{metric.Id},{metric.MetricName},{metric.Category},{metric.Value}," +
                          $"{metric.Unit},{metric.Timestamp:O},{metric.Dimension1}," +
                          $"{metric.Dimension2},{metric.Dimension3}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateJsonContent(List<Domain.Entities.AnalyticsMetric> metrics)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(metrics);
        return Encoding.UTF8.GetBytes(json);
    }
}
