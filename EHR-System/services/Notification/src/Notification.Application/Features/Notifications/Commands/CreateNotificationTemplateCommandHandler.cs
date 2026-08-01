namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for creating notification template
/// </summary>
public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, CreateNotificationTemplateResponse>
{
    private readonly ILogger<CreateNotificationTemplateCommandHandler> _logger;

    public CreateNotificationTemplateCommandHandler(ILogger<CreateNotificationTemplateCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<CreateNotificationTemplateResponse> Handle(
        CreateNotificationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating notification template {TemplateName}", command.TemplateName);

        try
        {
            // TODO: Implement template creation logic
            // - Validate template name is unique
            // - Create template entity
            // - Save to repository
            // - Publish TemplateCreatedEvent
            // - Cache template

            var templateId = Guid.NewGuid();

            return new CreateNotificationTemplateResponse(
                Success: true,
                Message: "Template created successfully",
                TemplateId: templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification template {TemplateName}", command.TemplateName);
            return new CreateNotificationTemplateResponse(
                Success: false,
                Message: $"Failed to create template: {ex.Message}");
        }
    }
}
