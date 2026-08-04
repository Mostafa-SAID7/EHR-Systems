namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when appointment is cancelled in Appointment service
/// </summary>
public class AppointmentCancelledIntegrationEventHandler : INotificationHandler<AppointmentCancelledIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AppointmentCancelledIntegrationEventHandler> _logger;

    public AppointmentCancelledIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<AppointmentCancelledIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(AppointmentCancelledIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing AppointmentCancelled event: {AppointmentId}", notification.AppointmentId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for cancelled appointment
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "AppointmentCancelled",
                Category = "Appointments",
                Value = 1,
                Unit = "count",
                Timestamp = notification.CancelledDate,
                Dimension1 = notification.DoctorId.ToString(),
                Dimension2 = notification.PatientId.ToString(),
                Dimension3 = notification.CancelledReason,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded AppointmentCancelled metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AppointmentCancelled event: {AppointmentId}", 
                notification.AppointmentId);
        }
    }
}

/// <summary>
/// Integration event from Appointment service
/// </summary>
public class AppointmentCancelledIntegrationEvent : INotification
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string CancelledReason { get; set; } = string.Empty;
    public DateTime CancelledDate { get; set; }
    public long TenantId { get; set; }
}
