using EHRPlatform.Services.Appointment.Domain.Enums;
using EHRPlatform.Services.Appointment.Services.Notifications;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Services;

/// <summary>
/// Background service for processing appointment reminders.
/// Runs periodically to send scheduled reminders via notification providers.
/// </summary>
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Check every 5 minutes

    public ReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder background service starting");

        // Initial delay to allow system startup
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminders in background service");
            }

            // Wait before next check
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Reminder background service stopping");
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var orchestrator = scope.ServiceProvider.GetRequiredService<NotificationOrchestrator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReminderBackgroundService>>();

            try
            {
                var reminderRepo = unitOfWork.Repository<AppointmentReminder>();
                var appointmentRepo = unitOfWork.Repository<Appointment>();
                var now = DateTime.UtcNow;

                // Get all pending reminders
                var pendingReminders = await reminderRepo.ListAsync(
                    q => q.Where(r =>
                        r.Status == ReminderStatus.Scheduled &&
                        r.ReminderTime <= now &&
                        !r.IsSent),
                    cancellationToken);

                if (pendingReminders.Count == 0)
                {
                    return;
                }

                logger.LogInformation("Processing {ReminderCount} pending reminders", pendingReminders.Count);

                var sentCount = 0;
                foreach (var reminder in pendingReminders)
                {
                    try
                    {
                        // Get appointment details
                        var appointment = await appointmentRepo.FirstOrDefaultAsync(
                            q => q.Where(a => a.Id == reminder.AppointmentId),
                            cancellationToken);

                        if (appointment == null)
                        {
                            logger.LogWarning("Appointment {AppointmentId} not found for reminder {ReminderId}",
                                reminder.AppointmentId, reminder.Id);
                            continue;
                        }

                        // Prepare template variables
                        var templateVars = new Dictionary<string, string>
                        {
                            { "PatientName", "Patient" }, // Would fetch real name from patient service
                            { "ProviderName", "Provider" }, // Would fetch real name from provider service
                            { "AppointmentDate", appointment.ScheduledStart.ToString("g") },
                            { "AppointmentType", appointment.AppointmentType.ToString() }
                        };

                        // Send notification via appropriate provider
                        var messageId = await orchestrator.SendReminderAsync(
                            "recipient@example.com", // Would be actual user email/phone
                            reminder.Method.ToString(),
                            "AppointmentReminder",
                            templateVars,
                            cancellationToken);

                        // Mark as sent
                        reminder.IsSent = true;
                        reminder.SentAt = DateTime.UtcNow;
                        reminder.Status = ReminderStatus.Sent;

                        await reminderRepo.UpdateAsync(reminder, cancellationToken);
                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send reminder {ReminderId}", reminder.Id);

                        // Mark as failed but keep retrying
                        reminder.Status = ReminderStatus.Failed;
                        await reminderRepo.UpdateAsync(reminder, cancellationToken);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }

                if (sentCount > 0)
                {
                    logger.LogInformation("Successfully sent {SentCount} of {TotalCount} reminders", sentCount, pendingReminders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending reminders");
            }
        }
    }

    /// <summary>
    /// Schedule a reminder for a specific appointment.
    /// Automatically schedules reminders 24 hours and 1 hour before the appointment.
    /// </summary>
    public static void ScheduleDefaultReminders(
        Guid appointmentId,
        DateTime appointmentTime,
        IReminderService reminderService)
    {
        // Schedule 24-hour reminder
        var oneDayBefore = appointmentTime.AddHours(-24);
        if (oneDayBefore > DateTime.UtcNow)
        {
            reminderService.ScheduleReminderAsync(appointmentId, oneDayBefore, ReminderType.Email).GetAwaiter().GetResult();
        }

        // Schedule 1-hour reminder
        var oneHourBefore = appointmentTime.AddHours(-1);
        if (oneHourBefore > DateTime.UtcNow)
        {
            reminderService.ScheduleReminderAsync(appointmentId, oneHourBefore, ReminderType.SMS).GetAwaiter().GetResult();
        }
    }
}
