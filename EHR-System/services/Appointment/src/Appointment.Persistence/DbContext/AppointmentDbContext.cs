using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Appointment.Domain.Entities;

namespace EHRPlatform.Services.Appointment.Persistence.DbContext;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added during migration
    // public DbSet<Appointment> Appointments { get; set; }
    // public DbSet<AppointmentSlot> AppointmentSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities here
    }
}
