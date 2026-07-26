using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Notification.Domain.Entities;

namespace EHRPlatform.Services.Notification.Data.Seeds;

/// <summary>
/// Seed data for Notification (Notification templates and examples).
/// </summary>
public static class NotificationSeed
{
    public static void SeedNotifications(this ModelBuilder modelBuilder)
    {
        var notificationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var recipientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<NotificationEntity>().HasData(
            new NotificationEntity
            {
                Id = notificationId,
                RecipientId = recipientId,
                Channel = "Email",
                NotificationType = "AppointmentReminder",
                Subject = "Appointment Reminder",
                Body = "You have an upcoming appointment tomorrow at 10:00 AM",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            }
        );
    }
}
