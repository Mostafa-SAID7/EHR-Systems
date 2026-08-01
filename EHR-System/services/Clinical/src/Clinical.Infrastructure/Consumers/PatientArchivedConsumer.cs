using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EHRPlatform.BuildingBlocks.EventBus.Events;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Infrastructure.Consumers;

/// <summary>
/// Consumes PatientArchivedIntegrationEvent from the event bus.
/// Locks all open clinical notes when a patient is archived.
/// </summary>
public class PatientArchivedConsumer : IConsumer<PatientArchivedIntegrationEvent>
{
    private readonly ClinicalContext _context;
    private readonly ILogger<PatientArchivedConsumer> _logger;

    public PatientArchivedConsumer(
        ClinicalContext context,
        ILogger<PatientArchivedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PatientArchivedIntegrationEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Clinical service received PatientArchived — PatientId: {PatientId}",
            evt.PatientId);

        // Lock all draft notes for archived patient
        var draftNotes = await _context.ClinicalNotes
            .Where(n => n.PatientId == evt.PatientId && n.Status == "Draft")
            .ToListAsync(context.CancellationToken);

        foreach (var note in draftNotes)
        {
            note.Status    = "Locked";
            note.UpdatedAt = DateTime.UtcNow;
        }

        if (draftNotes.Any())
        {
            await _context.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation(
                "{Count} draft clinical notes locked for archived Patient {PatientId}",
                draftNotes.Count, evt.PatientId);
        }
    }
}
