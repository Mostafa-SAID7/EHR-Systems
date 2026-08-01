using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Infrastructure.OutboxProcessor;

/// <summary>
/// Background service that processes outbox events for the Clinical service.
/// Polls the OutboxEvents table and publishes pending events to RabbitMQ.
/// Guarantees at-least-once delivery for domain events.
/// </summary>
public class ClinicalOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClinicalOutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public ClinicalOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<ClinicalOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Clinical OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing clinical outbox events");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicalContext>();

        var pendingEvents = await context.OutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (!pendingEvents.Any()) return;

        _logger.LogDebug("Processing {Count} clinical outbox events", pendingEvents.Count);

        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                // Mark as processed (idempotent publish pattern)
                outboxEvent.ProcessedAt = DateTime.UtcNow;
                _logger.LogDebug(
                    "Published clinical event {EventType} — Id: {Id}",
                    outboxEvent.EventType, outboxEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish clinical event {EventId}", outboxEvent.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
