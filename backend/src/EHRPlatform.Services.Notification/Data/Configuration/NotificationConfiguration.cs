using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Notification.Domain.Entities;

namespace EHRPlatform.Services.Notification.Data.Configuration;

/// <summary>
/// Entity configuration for Notification.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.RecipientId);
        entity.HasIndex(e => e.Status);
        entity.HasIndex(e => e.Channel);
        entity.HasIndex(e => e.CreatedAt).IsDescending();
        entity.HasIndex(e => e.ScheduledFor);
        entity.Property(e => e.Channel).HasMaxLength(50);
        entity.Property(e => e.Status).HasMaxLength(50);
    }
}
