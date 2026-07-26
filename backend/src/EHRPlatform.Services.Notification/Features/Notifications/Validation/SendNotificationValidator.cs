using FluentValidation;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;

namespace EHRPlatform.Services.Notification.Features.Notifications.Validation;

public class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Channel).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}
