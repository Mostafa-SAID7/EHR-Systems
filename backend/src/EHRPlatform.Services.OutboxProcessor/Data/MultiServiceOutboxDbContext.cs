using EHRPlatform.BuildingBlocks.Common.Events;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Common.Data;

/// <summary>
/// DbContext that reads OutboxEvents from all microservices' databases.
/// This is used by the Outbox Processor worker to poll and publish events.
/// 
/// Since all services share the same schema (EHRPlatform.Common.Events.OutboxEvent),
/// this context connects to PostgreSQL and reads from the OutboxEvents table
/// (which exists in each service's database due to shared migrations).
/// </summary>
public class MultiServiceOutboxDbContext : DbContext
{
    public MultiServiceOutboxDbContext(DbContextOptions<MultiServiceOutboxDbContext> options)
        : base(options)
    {
    }

    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure OutboxEvent entity
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.AggregateId)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.EventData)
                .IsRequired()
                .HasColumnType("jsonb");

            entity.Property(e => e.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.PublishAttempts)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes for efficient querying
            entity.HasIndex(e => new { e.IsPublished, e.PublishAttempts, e.CreatedAt })
                .HasName("IX_OutboxEvent_Unpublished");

            entity.HasIndex(e => e.AggregateId)
                .HasName("IX_OutboxEvent_AggregateId");

            entity.HasIndex(e => e.EventType)
                .HasName("IX_OutboxEvent_EventType");

            entity.HasIndex(e => e.CreatedAt)
                .HasName("IX_OutboxEvent_CreatedAt");
        });
    }
}

