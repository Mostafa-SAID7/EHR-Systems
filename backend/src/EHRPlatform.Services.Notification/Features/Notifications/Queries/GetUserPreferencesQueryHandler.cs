using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;

namespace EHRPlatform.Services.Notification.Features.Notifications.Queries;

/// <summary>
/// Get notification preferences for a user handler.
/// Single Responsibility: Retrieve channel/type preferences for the given user.
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


