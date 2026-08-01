namespace EHRPlatform.Services.Notification.Contracts.Requests;

/// <summary>
/// Request DTO for creating notification template
/// </summary>
public class CreateNotificationTemplateRequestDto
{
    /// <summary>Gets or sets template name.</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets email/message subject.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Gets or sets template body content.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets content type (HTML, PlainText, Markdown).</summary>
    public string? ContentType { get; set; }
}
