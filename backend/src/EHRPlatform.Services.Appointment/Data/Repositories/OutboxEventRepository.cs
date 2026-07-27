using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Appointment.Data.Repositories;

public interface IOutboxEventRepository
{
    Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync(int batchSize = 100);
    Task MarkProcessedAsync(Guid eventId);
}

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly AppointmentContext _context;
    private readonly ILogger<OutboxEventRepository> _logger;

    public OutboxEventRepository(AppointmentContext context, ILogger<OutboxEventRepository> logger)
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
                _logger.LogInformation("OutboxEvent {eventId} marked as published", eventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking OutboxEvent {eventId} as published", eventId);
            throw;
        }
    }
}
