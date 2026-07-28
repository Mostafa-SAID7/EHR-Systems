using EHRPlatform.Common.Data;
using EHRPlatform.Services.Notification.Data.Documents;

namespace EHRPlatform.Services.Notification.Data.Repositories;

/// <summary>
/// MongoDB implementation of INotificationMongoRepository.
/// Uses two separate collections: one for notifications and one for preferences.
/// </summary>
public sealed class NotificationMongoRepository : INotificationMongoRepository
{
    private readonly IMongoRepository<NotificationDocument>           _notifRepo;
    private readonly IMongoRepository<NotificationPreferenceDocument> _prefRepo;

    public NotificationMongoRepository(
        IMongoRepository<NotificationDocument>           notifRepo,
        IMongoRepository<NotificationPreferenceDocument> prefRepo)
    {
        _notifRepo = notifRepo;
        _prefRepo  = prefRepo;
    }

    // ── Notifications ─────────────────────────────────────────────────────────

    public Task<NotificationDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _notifRepo.GetByEntityIdAsync(id, ct);

    public async Task<IEnumerable<NotificationDocument>> GetByRecipientAsync(
        Guid recipientId, int page, int size, CancellationToken ct = default)
    {
        var result = await _notifRepo.GetPagedAsync(
            page, size,
            filter: n => n.RecipientId == recipientId && n.DeletedAt == null,
            cancellationToken: ct);
        return result.items;
    }

    public Task InsertAsync(NotificationDocument doc, CancellationToken ct = default)
        => _notifRepo.InsertAsync(doc, ct);

    public Task ReplaceAsync(NotificationDocument doc, CancellationToken ct = default)
    {
        doc.UpdatedAt = DateTime.UtcNow;
        return _notifRepo.ReplaceAsync(doc, ct);
    }

    // ── Preferences ───────────────────────────────────────────────────────────

    public Task<NotificationPreferenceDocument?> GetPreferenceAsync(
        Guid userId, string channel, string notificationType, CancellationToken ct = default)
        => _prefRepo.FindOneAsync(
            p => p.UserId == userId
              && p.Channel == channel
              && p.NotificationType == notificationType
              && p.DeletedAt == null,
            ct);

    public Task<IEnumerable<NotificationPreferenceDocument>> GetUserPreferencesAsync(
        Guid userId, CancellationToken ct = default)
        => _prefRepo.FindAsync(
            p => p.UserId == userId && p.DeletedAt == null, ct);

    public async Task UpsertPreferenceAsync(
        NotificationPreferenceDocument doc, CancellationToken ct = default)
    {
        var existing = await GetPreferenceAsync(
            doc.UserId, doc.Channel, doc.NotificationType, ct);

        if (existing == null)
        {
            await _prefRepo.InsertAsync(doc, ct);
        }
        else
        {
            existing.IsEnabled  = doc.IsEnabled;
            existing.UpdatedAt  = DateTime.UtcNow;
            await _prefRepo.ReplaceAsync(existing, ct);
        }
    }
}
