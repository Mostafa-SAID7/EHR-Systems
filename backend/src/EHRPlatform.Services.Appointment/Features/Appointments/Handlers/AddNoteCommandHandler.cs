using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Add note to appointment handler.
/// </summary>
public class AddNoteCommandHandler : ICommandHandler<AddNoteCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddNoteCommandHandler> _logger;

    public AddNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AddNoteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding note to appointment {AppointmentId}",
            command.AppointmentId);

        var repo = _unitOfWork.Repository<Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        // Parse privacy level
        var privacyLevel = Enum.TryParse<NotePrivacyLevel>(command.PrivacyLevel ?? "InternalOnly", out var level)
            ? level
            : NotePrivacyLevel.InternalOnly;

        // Add note
        appointment.AddNote(
            command.Content,
            command.CreatedById,
            privacyLevel,
            command.Category);

        await repo.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Note added successfully to appointment {AppointmentId}",
            command.AppointmentId);
    }
}

