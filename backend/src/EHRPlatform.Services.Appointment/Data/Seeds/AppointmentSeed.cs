using Microsoft.EntityFrameworkCore;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Data.Seeds;

/// <summary>
/// Seed data for Appointments, Reminders, and Provider Availability.
/// </summary>
public static class AppointmentSeed
{
    public static void SeedAppointments(this ModelBuilder modelBuilder)
    {
        var appointmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var reminderId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var providerId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<ApptEntity>().HasData(
            new ApptEntity
            {
                Id = appointmentId,
                PatientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProviderId = providerId,
                ScheduledStart = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
                ScheduledEnd = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc),
                Status = "Scheduled",
                AppointmentType = "Office",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<AppointmentReminder>().HasData(
            new AppointmentReminder
            {
                Id = reminderId,
                AppointmentId = appointmentId,
                Method = "Email",
                ReminderTime = new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
                IsSent = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<ProviderAvailability>().HasData(
            new ProviderAvailability
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ProviderId = providerId,
                SlotStart = new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
                SlotEnd = new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
