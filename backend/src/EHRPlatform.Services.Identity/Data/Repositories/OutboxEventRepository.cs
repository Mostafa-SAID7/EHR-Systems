using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Events;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Identity.Data.Repositories;

public interface IOutboxEventRepository
{
    Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync(int batchSize = 100);
    Task MarkProcessedAsync(Guid eventId);
}

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly IdentityContext _context;
    private readonly ILogger<OutboxEventRepository> _logger;

    public OutboxEventRepository(IdentityContext context, ILogger<OutboxEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking OutboxEvent {eventId} as published", eventId);
            throw;
        }
    }
}
