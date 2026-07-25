using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// Appointment alias comes from GlobalUsings.cs

namespace EHRPlatform.Services.Appointment.Data.Configuration;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.PatientId);
        entity.HasIndex(e => e.ProviderId);
        entity.HasIndex(e => e.ScheduledStart).IsDescending();
        entity.HasIndex(e => new { e.ProviderId, e.ScheduledStart });
        entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");
        entity.Property(e => e.AppointmentType).HasMaxLength(50);
    }
}
