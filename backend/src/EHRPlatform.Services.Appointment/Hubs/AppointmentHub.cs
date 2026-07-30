using Microsoft.AspNetCore.SignalR;

namespace EHRPlatform.Services.Appointment.Hubs;

/// <summary>
/// SignalR hub for real-time appointment updates.
/// Handles connections and broadcasts appointment changes to connected clients.
/// </summary>
public class AppointmentHub : Hub<IAppointmentHubClient>
{
    private readonly ILogger<AppointmentHub> _logger;

    public AppointmentHub(ILogger<AppointmentHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to appointment updates for a specific appointment.
    /// </summary>
    public async Task SubscribeToAppointment(string appointmentId)
    {
        try
        {
            // Add connection to appointment group
            var groupName = $"appointment-{appointmentId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "Client {ConnectionId} subscribed to appointment {AppointmentId}",
                Context.ConnectionId, appointmentId);

            // Notify subscribers
            await Clients.Group(groupName).UserJoined(new
            {
                userId = Context.ConnectionId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribe from appointment updates.
    /// </summary>
    public async Task UnsubscribeFromAppointment(string appointmentId)
    {
        try
        {
            var groupName = $"appointment-{appointmentId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "Client {ConnectionId} unsubscribed from appointment {AppointmentId}",
                Context.ConnectionId, appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Called when a client connects.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}

/// <summary>
/// SignalR client interface for appointment hub methods.
/// Defines methods that can be called from server to client.
/// </summary>
public interface IAppointmentHubClient
{
    /// <summary>
    /// Notify of appointment scheduled.
    /// </summary>
    Task AppointmentScheduled(object appointment);

    /// <summary>
    /// Notify of appointment confirmed.
    /// </summary>
    Task AppointmentConfirmed(string appointmentId);

    /// <summary>
    /// Notify of appointment cancelled.
    /// </summary>
    Task AppointmentCancelled(string appointmentId, string reason);

    /// <summary>
    /// Notify of appointment status changed.
    /// </summary>
    Task AppointmentStatusChanged(string appointmentId, string newStatus);

    /// <summary>
    /// Notify of reminder sent.
    /// </summary>
    Task ReminderSent(string appointmentId, string reminderType);

    /// <summary>
    /// Notify of note added.
    /// </summary>
    Task NoteAdded(string appointmentId, object note);

    /// <summary>
    /// Notify of appointment rescheduled.
    /// </summary>
    Task AppointmentRescheduled(string appointmentId, object rescheduleInfo);

    /// <summary>
    /// Notify that a user has joined the appointment.
    /// </summary>
    Task UserJoined(object userInfo);

    /// <summary>
    /// Notify of error.
    /// </summary>
    Task NotifyError(string message);
}

/// <summary>
/// Extension methods for sending hub notifications.
/// </summary>
public static class AppointmentHubExtensions
{
    private const string HubGroupPrefix = "appointment-";

    /// <summary>
    /// Notify appointment scheduled.
    /// </summary>
    public static async Task NotifyAppointmentScheduled(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId,
        object appointment)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).AppointmentScheduled(appointment);
    }

    /// <summary>
    /// Notify appointment confirmed.
    /// </summary>
    public static async Task NotifyAppointmentConfirmed(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).AppointmentConfirmed(appointmentId);
    }

    /// <summary>
    /// Notify appointment cancelled.
    /// </summary>
    public static async Task NotifyAppointmentCancelled(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId,
        string reason)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).AppointmentCancelled(appointmentId, reason);
    }

    /// <summary>
    /// Notify status changed.
    /// </summary>
    public static async Task NotifyStatusChanged(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId,
        string newStatus)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).AppointmentStatusChanged(appointmentId, newStatus);
    }

    /// <summary>
    /// Notify note added.
    /// </summary>
    public static async Task NotifyNoteAdded(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId,
        object note)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).NoteAdded(appointmentId, note);
    }

    /// <summary>
    /// Notify rescheduled.
    /// </summary>
    public static async Task NotifyRescheduled(
        this IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
        string appointmentId,
        object rescheduleInfo)
    {
        var groupName = $"{HubGroupPrefix}{appointmentId}";
        await hubContext.Clients.Group(groupName).AppointmentRescheduled(appointmentId, rescheduleInfo);
    }
}
