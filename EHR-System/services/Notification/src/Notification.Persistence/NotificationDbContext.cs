namespace EHRPlatform.Services.Notification.Persistence;

using EHRPlatform.Services.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for Notification microservice.
/// </summary>
public interface INotificationDbContext
{
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class NotificationDbContext : DbContext, INotificationDbContext
{
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.Channel);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.NextRetryAt });
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Channel).HasMaxLength(20);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.MessageId).HasMaxLength(255);
        });

        // NotificationPreference
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Channel, e.NotificationType }).IsUnique();
            entity.Property(e => e.Channel).HasMaxLength(20);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
        });
    }
}
