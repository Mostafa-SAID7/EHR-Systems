namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for getting notification templates
/// </summary>
public class GetNotificationTemplatesQueryHandler : IRequestHandler<GetNotificationTemplatesQuery, GetNotificationTemplatesResponse>
{
    private readonly ILogger<GetNotificationTemplatesQueryHandler> _logger;

    public GetNotificationTemplatesQueryHandler(ILogger<GetNotificationTemplatesQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<GetNotificationTemplatesResponse> Handle(
        GetNotificationTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving notification templates");

        try
        {
            // TODO: Implement query logic
            // - Query templates from repository
            // - Paginate results
            // - Cache results (1 hour)
            // - Return paginated response

            var templates = new List<NotificationTemplateDto>();

            return new GetNotificationTemplatesResponse(
                Success: true,
                Message: "Templates retrieved successfully",
                Templates: templates,
                TotalCount: 0,
                PageNumber: query.PageNumber,
                PageSize: query.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification templates");
            return new GetNotificationTemplatesResponse(
                Success: false,
                Message: $"Failed to retrieve templates: {ex.Message}",
                Templates: Enumerable.Empty<NotificationTemplateDto>(),
                TotalCount: 0,
                PageNumber: query.PageNumber,
                PageSize: query.PageSize);
        }
    }
}
