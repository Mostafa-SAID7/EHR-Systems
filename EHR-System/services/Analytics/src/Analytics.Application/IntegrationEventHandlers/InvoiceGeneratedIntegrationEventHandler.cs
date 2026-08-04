namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when invoice is generated in Billing service
/// </summary>
public class InvoiceGeneratedIntegrationEventHandler : INotificationHandler<InvoiceGeneratedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<InvoiceGeneratedIntegrationEventHandler> _logger;

    public InvoiceGeneratedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<InvoiceGeneratedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(InvoiceGeneratedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing InvoiceGenerated event: {InvoiceId}", notification.InvoiceId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for generated invoice
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "InvoiceGenerated",
                Category = "Revenue",
                Value = notification.Amount,
                Unit = "currency",
                Timestamp = notification.GeneratedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.InvoiceStatus,
                Dimension3 = notification.ServiceType,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded InvoiceGenerated metric: {MetricId} Amount: {Amount}", 
                metric.Id, notification.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InvoiceGenerated event: {InvoiceId}", 
                notification.InvoiceId);
        }
    }
}

/// <summary>
/// Integration event from Billing service
/// </summary>
public class InvoiceGeneratedIntegrationEvent : INotification
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public long TenantId { get; set; }
}
