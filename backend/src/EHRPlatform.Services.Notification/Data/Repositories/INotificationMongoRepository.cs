using EHRPlatform.Services.Notification.Data.Documents;

namespace EHRPlatform.Services.Notification.Data.Repositories;

/// <summary>
/// MongoDB-backed repository for Notification and NotificationPreference documents.
/// TemplateVars dictionary and per-channel payload variations make MongoDB
/// the natural fit here — no nullable columns, no schema migrations per type.
/// </summary>
public interface INotificationMongoRepository
{
    // ── Notifications ─────────────────────────────────────────────────────────
    Task<NotificationDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<NotificationDocument>> GetByRecipientAsync(
        Guid recipientId, int page, int size, CancellationToken ct = default);

    Task InsertAsync(NotificationDocument doc, CancellationToken ct = default);
    Task ReplaceAsync(NotificationDocument doc, CancellationToken ct = default);

    // ── Preferences ───────────────────────────────────────────────────────────
    Task<NotificationPreferenceDocument?> GetPreferenceAsync(
        Guid userId, string channel, string notificationType, CancellationToken ct = default);

    Task<IEnumerable<NotificationPreferenceDocument>> GetUserPreferencesAsync(
        Guid userId, CancellationToken ct = default);

    Task UpsertPreferenceAsync(NotificationPreferenceDocument doc, CancellationToken ct = default);
}
