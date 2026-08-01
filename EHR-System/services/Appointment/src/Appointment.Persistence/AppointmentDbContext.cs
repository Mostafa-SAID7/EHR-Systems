namespace EHRPlatform.Services.Appointment.Persistence;

using EHRPlatform.Services.Appointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for Appointment microservice.
/// </summary>
public interface IAppointmentDbContext
{
    DbSet<Appointment> Appointments { get; }
    DbSet<AppointmentReminder> AppointmentReminders { get; }
    DbSet<AppointmentNote> AppointmentNotes { get; }
    DbSet<RescheduleHistory> RescheduleHistories { get; }
    DbSet<ProviderAvailability> ProviderAvailabilities { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class AppointmentDbContext : DbContext, IAppointmentDbContext
{
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<AppointmentReminder> AppointmentReminders { get; set; } = null!;
    public DbSet<AppointmentNote> AppointmentNotes { get; set; } = null!;
    public DbSet<RescheduleHistory> RescheduleHistories { get; set; } = null!;
    public DbSet<ProviderAvailability> ProviderAvailabilities { get; set; } = null!;

    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ProviderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ScheduledStart, e.ScheduledEnd });
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasMany(e => e.Reminders).WithOne(r => r.Appointment).HasForeignKey(r => r.AppointmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Notes_Collection).WithOne(n => n.Appointment).HasForeignKey(n => n.AppointmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.RescheduleHistory).WithOne(rh => rh.Appointment).HasForeignKey(rh => rh.AppointmentId).OnDelete(DeleteBehavior.Cascade);
        });

        // AppointmentReminder
        modelBuilder.Entity<AppointmentReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId);
            entity.HasIndex(e => new { e.Status, e.ScheduledReminderTime });
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.ReminderMethod).HasMaxLength(20);
        });

        // AppointmentNote
        modelBuilder.Entity<AppointmentNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId);
            entity.Property(e => e.PrivacyLevel).HasMaxLength(20);
        });

        // RescheduleHistory
        modelBuilder.Entity<RescheduleHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId);
        });

        // ProviderAvailability
        modelBuilder.Entity<ProviderAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProviderId);
            entity.HasIndex(e => new { e.ProviderId, e.DayOfWeek });
        });
    }
}
