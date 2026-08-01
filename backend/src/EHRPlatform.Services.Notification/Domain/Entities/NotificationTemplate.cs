using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Notification.Domain.Entities;

/// <summary>
/// Notification template for reusable messages.
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push
    public string NotificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty; // With {{variable}} placeholders
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Render template with variables.
    /// </summary>
    public string RenderBody(Dictionary<string, string> variables)
    {
        var body = BodyTemplate;
        foreach (var (key, value) in variables)
        {
            body = body.Replace($"{{{{{key}}}}}", value ?? "");
        }
        return body;
    }
}


