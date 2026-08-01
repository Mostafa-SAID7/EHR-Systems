using Microsoft.EntityFrameworkCore;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Events;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;
using ProvAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Data;

/// <summary>DbContext for Appointment Service.</summary>
public class AppointmentContext : BaseDbContext
{
    public AppointmentContext(DbContextOptions<AppointmentContext> options) : base(options) { }

    public DbSet<ApptEntity> Appointments { get; set; } = null!;
    public DbSet<AppointmentReminder> AppointmentReminders { get; set; } = null!;
    public DbSet<ProvAvailEntity> ProviderAvailability { get; set; } = null!;
    
    // ✓ Outbox Event Pattern - Ensures consistency across stores
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new Configuration.AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.AppointmentReminderConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.ProviderAvailabilityConfiguration());
        modelBuilder.SeedAppointments();
    }
}

