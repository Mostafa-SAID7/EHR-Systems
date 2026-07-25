using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;

using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;
using Mapster;

namespace EHRPlatform.Services.Notification.Features.Notifications.Queries;

/// <summary>
/// Get notification by ID handler.
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

        var repo = _unitOfWork.Repository<Notification>();
        var notification = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == request.NotificationId),
            cancellationToken);

        if (notification == null)
            throw new InvalidOperationException($"Notification {request.NotificationId} not found");

        return notification.Adapt<NotificationResponseDto>();
    }
}

/// <summary>
/// Get user notifications handler.
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

        var repo = _unitOfWork.Repository<Notification>();
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

/// <summary>
/// Get user preferences handler.
/// </summary>
public class GetUserPreferencesQueryHandler : IQueryHandler<GetUserPreferencesQuery, List<PreferenceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserPreferencesQueryHandler> _logger;

    public GetUserPreferencesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserPreferencesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<PreferenceDto>> Handle(
        GetUserPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching preferences for user {UserId}", request.UserId);

        var prefRepo = _unitOfWork.Repository<NotificationPreference>();
        var preferences = await prefRepo.ToListAsync(
            q => q.Where(p => p.UserId == request.UserId),
            cancellationToken);

        return preferences.Select(p => new PreferenceDto
        {
            Channel = p.Channel,
            NotificationType = p.NotificationType,
            IsEnabled = p.IsEnabled
        }).ToList();
    }
}
