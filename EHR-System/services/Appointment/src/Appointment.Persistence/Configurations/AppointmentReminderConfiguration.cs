using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Data.Configuration;

public class AppointmentReminderConfiguration : IEntityTypeConfiguration<AppointmentReminder>
{
    public void Configure(EntityTypeBuilder<AppointmentReminder> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Appointment)
            .WithMany(a => a.Reminders)
            .HasForeignKey(e => e.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(e => e.AppointmentId);
        entity.HasIndex(e => e.ReminderTime);
        entity.Property(e => e.Method).HasMaxLength(50);
    }
}
