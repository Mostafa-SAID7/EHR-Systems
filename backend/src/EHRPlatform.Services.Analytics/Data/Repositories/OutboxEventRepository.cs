using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Implementations;
using EHRPlatform.Common.Events;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Analytics.Data.Repositories;

/// <summary>
/// Repository for managing OutboxEvent entries.
/// Implements the Outbox Pattern for ensuring consistency between services.
/// </summary>
public interface IOutboxEventRepository
{
    Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync(int batchSize = 100);
    Task MarkProcessedAsync(Guid eventId);
}

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly AnalyticsContext _context;
    private readonly ILogger<OutboxEventRepository> _logger;

    public OutboxEventRepository(AnalyticsContext context, ILogger<OutboxEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets unprocessed OutboxEvents for replication to other stores/services.
    /// </summary>
    public async Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync(int batchSize = 100)
    {
        try
        {
            return await _context.OutboxEvents
                .Where(e => !e.IsPublished && e.ShouldRetry)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unprocessed OutboxEvents");
            throw;
        }
    }

    /// <summary>
    /// Marks an OutboxEvent as processed by setting IsPublished and PublishedAt.
    /// </summary>
    public async Task MarkProcessedAsync(Guid eventId)
    {
        try
        {
            var @event = await _context.OutboxEvents.FindAsync(eventId);
            if (@event != null)
            {
                @event.IsPublished = true;
                @event.PublishedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("OutboxEvent {eventId} marked as published", eventId);
            }
            else
            {
                _logger.LogWarning("OutboxEvent {eventId} not found", eventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking OutboxEvent {eventId} as published", eventId);
            throw;
        }
    }
}

