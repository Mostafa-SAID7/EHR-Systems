namespace EHRPlatform.Services.Analytics.Infrastructure.Kafka;

using MassTransit;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Kafka consumer that aggregates metrics from domain events.
/// </summary>
public class AnalyticsEventConsumer : IConsumer<AnalyticsDomainEvent>
{
    private readonly IAnalyticsDbContext _context;
    private readonly ILogger<AnalyticsEventConsumer> _logger;

    public AnalyticsEventConsumer(IAnalyticsDbContext context, ILogger<AnalyticsEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AnalyticsDomainEvent> context)
    {
        _logger.LogInformation("Consuming analytics event: {EventType}", context.Message.EventType);

        try
        {
            // Record metric
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = context.Message.MetricName,
                Category = context.Message.Category,
                Value = context.Message.Value,
                Unit = context.Message.Unit,
                Dimension1 = context.Message.Dimension1,
                Dimension2 = context.Message.Dimension2,
                Dimension3 = context.Message.Dimension3,
                MetricDate = DateTime.UtcNow.Date,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsMetrics.Add(metric);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Metric recorded: {MetricName}={Value}", metric.MetricName, metric.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming analytics event");
        }
    }
}

/// <summary>
/// Domain event contract for analytics.
/// </summary>
public interface AnalyticsDomainEvent
{
    Guid EventId { get; }
    string EventType { get; }
    string MetricName { get; }
    string Category { get; }
    decimal Value { get; }
    string Unit { get; }
    string? Dimension1 { get; }
    string? Dimension2 { get; }
    string? Dimension3 { get; }
    DateTime OccurredAt { get; }
}
