namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when payment is processed in Payment service
/// </summary>
public class PaymentProcessedIntegrationEventHandler : INotificationHandler<PaymentProcessedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PaymentProcessedIntegrationEventHandler> _logger;

    public PaymentProcessedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<PaymentProcessedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing PaymentProcessed event: {PaymentId}", notification.PaymentId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for processed payment
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "PaymentProcessed",
                Category = "Revenue",
                Value = notification.Amount,
                Unit = "currency",
                Timestamp = notification.ProcessedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.PaymentMethod,
                Dimension3 = notification.PaymentStatus,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded PaymentProcessed metric: {MetricId} Amount: {Amount}", 
                metric.Id, notification.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentProcessed event: {PaymentId}", 
                notification.PaymentId);
        }
    }
}

/// <summary>
/// Integration event from Payment service
/// </summary>
public class PaymentProcessedIntegrationEvent : INotification
{
    public Guid PaymentId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
    public long TenantId { get; set; }
}
