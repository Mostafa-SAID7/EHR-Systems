namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;
using EHRPlatform.Services.Notification.Domain.Entities;
using EHRPlatform.Services.Notification.Persistence;
using EHRPlatform.Services.Notification.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Handler for SendNotificationCommand - Routes notification to appropriate channel.
/// </summary>
public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, SendNotificationResponse>
{
    private readonly INotificationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushService _pushService;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    public SendNotificationCommandHandler(
        INotificationDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        IPushService pushService,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task<SendNotificationResponse> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending notification: Channel={Channel}, Type={Type}, Recipient={RecipientId}",
            request.Channel, request.NotificationType, request.RecipientId);

        try
        {
            // Check user preference
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == request.RecipientId &&
                                         p.Channel == request.Channel &&
                                         p.NotificationType == request.NotificationType,
                    cancellationToken);

            if (preference != null && !preference.IsEnabled)
            {
                _logger.LogInformation("Notification blocked by user preference: {RecipientId}", request.RecipientId);
                return new SendNotificationResponse
                {
                    Success = false,
                    Message = "User has disabled this notification type"
                };
            }

            // Create notification record
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = request.RecipientId,
                Channel = request.Channel,
                NotificationType = request.NotificationType,
                Subject = request.Subject,
                Body = request.Body,
                ScheduledFor = request.ScheduledFor,
                Status = request.ScheduledFor != null ? "Scheduled" : "Pending",
                TemplateVariables = request.TemplateVariables != null ? JsonSerializer.Serialize(request.TemplateVariables) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            // If not scheduled, send immediately
            if (request.ScheduledFor == null || request.ScheduledFor <= DateTime.UtcNow)
            {
                await SendViaChannelAsync(notification, request.Channel, cancellationToken);
            }

            return new SendNotificationResponse
            {
                Success = true,
                NotificationId = notification.Id,
                Message = "Notification queued for delivery"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return new SendNotificationResponse
            {
                Success = false,
                Message = "An error occurred while sending the notification"
            };
        }
    }

    private async Task SendViaChannelAsync(Notification notification, string channel, CancellationToken cancellationToken)
    {
        try
        {
            string? messageId = null;

            switch (channel.ToLower())
            {
                case "email":
                    messageId = await _emailService.SendEmailAsync(notification.RecipientId, notification.Subject, notification.Body, cancellationToken);
                    break;

                case "sms":
                    messageId = await _smsService.SendSmsAsync(notification.RecipientId, notification.Body, cancellationToken);
                    break;

                case "push":
                    messageId = await _pushService.SendPushAsync(notification.RecipientId, notification.Subject, notification.Body, cancellationToken);
                    break;

                case "inapp":
                    messageId = Guid.NewGuid().ToString(); // InApp messages stored in DB directly
                    break;
            }

            if (!string.IsNullOrEmpty(messageId))
            {
                notification.Send(messageId);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Notification sent: {NotificationId}, MessageId: {MessageId}", notification.Id, messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification via {Channel}", channel);
            notification.MarkFailed($"Failed to send via {channel}: {ex.Message}");
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
