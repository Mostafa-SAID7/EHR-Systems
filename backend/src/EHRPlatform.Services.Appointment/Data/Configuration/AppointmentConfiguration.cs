using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Data.Configuration;

public class AppointmentConfiguration : IEntityTypeConfiguration<ApptEntity>
{
    public void Configure(EntityTypeBuilder<ApptEntity> entity)
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
