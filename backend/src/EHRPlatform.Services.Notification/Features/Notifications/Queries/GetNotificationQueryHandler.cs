using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using Mapster;

namespace EHRPlatform.Services.Notification.Features.Notifications.Queries;

/// <summary>
/// Get notification by ID query handler.
/// Single Responsibility: Fetch and project a single notification by its ID.
/// </summary>
public class GetNotificationQueryHandler : IQueryHandler<GetNotificationQuery, NotificationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetNotificationQueryHandler> _logger;

    public GetNotificationQueryHandler(IUnitOfWork unitOfWork, ILogger<GetNotificationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<NotificationResponseDto> Handle(
        GetNotificationQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching notification {NotificationId}", request.NotificationId);

        var repo = _unitOfWork.Repository<NotificationEntity>();
        var notification = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == request.NotificationId),
            cancellationToken);

        if (notification == null)
            throw new InvalidOperationException($"Notification {request.NotificationId} not found");

        return notification.Adapt<NotificationResponseDto>();
    }
}
