using EHRPlatform.Common.CQRS;
using FluentValidation;

namespace EHRPlatform.Services.Notification.Features.Notifications.Commands;

/// <summary>
/// Send notification command.
/// </summary>
public record SendNotificationCommand : ICommand<NotificationResponseDto>
{
    public Guid RecipientId { get; init; }
    public string Channel { get; init; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Dictionary<string, string>? TemplateVars { get; init; }
    public string? Recipient { get; init; } // Email, phone, device token
    public DateTime? ScheduledFor { get; init; }
}

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Channel).Must(c => new[] { "Email", "SMS", "Push", "InApp" }.Contains(c));
        RuleFor(x => x.NotificationType).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Body).NotEmpty();
    }
}
