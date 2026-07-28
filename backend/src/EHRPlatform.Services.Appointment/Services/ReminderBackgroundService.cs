using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Services;

/// <summary>
/// Background service for processing appointment reminders.
/// Runs periodically to send scheduled reminders.
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
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();

            try
            {
                var sentCount = await reminderService.SendPendingRemindersAsync(cancellationToken);

                if (sentCount > 0)
                {
                    _logger.LogInformation("Sent {SentCount} reminders", sentCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pending reminders");
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
