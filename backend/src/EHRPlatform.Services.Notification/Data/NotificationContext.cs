using Microsoft.EntityFrameworkCore;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Notification.Domain.Entities;

namespace EHRPlatform.Services.Notification.Data;

/// <summary>
/// DbContext for Notification Service.
/// Single Responsibility: Configure entity mappings and relationships.
/// </summary>
public class NotificationContext : BaseDbContext
{
    public NotificationContext(DbContextOptions<NotificationContext> options) : base(options) { }

    public DbSet<NotificationEntity> Notifications { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;
    
    // ✓ Outbox Event Pattern
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationContext).Assembly);
    }
}

