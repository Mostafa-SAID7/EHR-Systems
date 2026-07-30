using EHRPlatform.Common.Events;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Default EF Core implementation of <see cref="IOutboxRepository"/>.
/// Services that use a custom <see cref="DbContext"/> can register this directly,
/// or provide their own implementation.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly DbContext _context;

    public OutboxRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<IEnumerable<OutboxEvent>> GetUnpublishedAsync(CancellationToken cancellationToken = default)
        => _context.Set<OutboxEvent>()
            .Where(e => !e.IsPublished && e.PublishAttempts < e.MaxPublishAttempts)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<OutboxEvent>)t.Result, cancellationToken);

    public Task<IEnumerable<OutboxEvent>> GetFailedAsync(CancellationToken cancellationToken = default)
        => _context.Set<OutboxEvent>()
            .Where(e => !e.IsPublished && e.PublishAttempts >= e.MaxPublishAttempts)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<OutboxEvent>)t.Result, cancellationToken);

    public async Task AddAsync(OutboxEvent @event, CancellationToken cancellationToken = default)
    {
        await _context.Set<OutboxEvent>().AddAsync(@event, cancellationToken);
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Set<OutboxEvent>()
            .FindAsync(new object[] { eventId }, cancellationToken);

        if (@event is null) return;

        @event.PublishedAt = DateTime.UtcNow;
        @event.IsPublished = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task IncrementAttemptAsync(
        Guid eventId,
        string failureReason,
        CancellationToken cancellationToken = default)
    {
        var @event = await _context.Set<OutboxEvent>()
            .FindAsync(new object[] { eventId }, cancellationToken);

        if (@event is null) return;

        @event.PublishAttempts++;
        @event.ErrorMessage = failureReason;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<OutboxEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _context.Set<OutboxEvent>().FindAsync(new object[] { eventId }, cancellationToken).AsTask();

    public async Task DeletePublishedOlderThanAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var old = await _context.Set<OutboxEvent>()
            .Where(e => e.IsPublished && e.PublishedAt < cutoff)
            .ToListAsync(cancellationToken);

        _context.Set<OutboxEvent>().RemoveRange(old);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
        => _context.Set<OutboxEvent>()
            .Where(e => !e.IsPublished)
            .CountAsync(cancellationToken);
}
