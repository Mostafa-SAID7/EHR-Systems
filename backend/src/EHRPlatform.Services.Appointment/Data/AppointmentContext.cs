using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
// Appointment alias comes from GlobalUsings.cs; AppointmentReminder and ProviderAvailability are fully resolved via global namespace import
using AppointmentReminder = EHRPlatform.Services.Appointment.Features.Appointments.Domain.AppointmentReminder;
using ProviderAvailability = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Data;

/// <summary>DbContext for Appointment Service.</summary>
public class AppointmentContext : BaseDbContext
{
    public AppointmentContext(DbContextOptions<AppointmentContext> options) : base(options) { }

    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<AppointmentReminder> AppointmentReminders { get; set; } = null!;
    public DbSet<ProviderAvailability> ProviderAvailability { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new Configuration.AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.AppointmentReminderConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.ProviderAvailabilityConfiguration());
        modelBuilder.SeedAppointments();
    }
}
