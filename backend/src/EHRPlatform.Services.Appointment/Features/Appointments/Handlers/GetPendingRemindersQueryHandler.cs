using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Get pending reminders query handler.
/// Retrieves all appointment reminders that are scheduled to be sent now or in the past.
/// </summary>
public class GetPendingRemindersQueryHandler : IQueryHandler<GetPendingRemindersQuery, IEnumerable<AppointmentReminderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPendingRemindersQueryHandler> _logger;

    public GetPendingRemindersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPendingRemindersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<AppointmentReminderDto>> Handle(
        GetPendingRemindersQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching pending reminders");

        var repo = _unitOfWork.Repository<AppointmentReminder>();
        var now = DateTime.UtcNow;

        var reminders = await repo.ListAsync(
            q => q.Where(r =>
                r.Status == Domain.Enums.ReminderStatus.Scheduled &&
                r.ReminderTime <= now &&
                !r.IsSent),
            cancellationToken);

        var result = reminders.Select(r => new AppointmentReminderDto
        {
            Id = r.Id,
            AppointmentId = r.AppointmentId,
            ReminderDateTime = r.ReminderTime,
            Channel = r.Method.ToString(),
            Status = r.Status.ToString(),
            SentAt = r.SentAt
        }).ToList();

        _logger.LogInformation("Found {ReminderCount} pending reminders", result.Count);

        return result;
    }
}
