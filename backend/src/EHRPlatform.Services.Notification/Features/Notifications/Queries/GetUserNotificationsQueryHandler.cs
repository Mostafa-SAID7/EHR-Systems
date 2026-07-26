using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using Mapster;

namespace EHRPlatform.Services.Notification.Features.Notifications.Queries;

/// <summary>
/// Get paginated notifications for a specific user handler.
/// Single Responsibility: Retrieve and paginate a user's notification history.
/// </summary>
public class GetUserNotificationsQueryHandler : IQueryHandler<GetUserNotificationsQuery, NotificationListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserNotificationsQueryHandler> _logger;

    public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserNotificationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<NotificationListDto> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching notifications for user {UserId}", request.UserId);

        var repo = _unitOfWork.Repository<NotificationEntity>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(n => n.RecipientId == request.UserId),
            cancellationToken);

        var notifications = await repo.ToListAsync(
            q => q.Where(n => n.RecipientId == request.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        return new NotificationListDto
        {
            Items = notifications.Adapt<List<NotificationResponseDto>>(),
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
