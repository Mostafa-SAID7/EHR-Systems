using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Billing.Domain.Entities;
using EHRPlatform.Services.Billing.Domain.Enums;
using EHRPlatform.Services.Billing.Features.Claims.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Claims.Handlers;

/// <summary>
/// Handler for processing Prior Authorization requests.
/// </summary>
public class RequestPriorAuthorizationCommandHandler : ICommandHandler<RequestPriorAuthorizationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<RequestPriorAuthorizationCommandHandler> _logger;

    public RequestPriorAuthorizationCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<RequestPriorAuthorizationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(RequestPriorAuthorizationCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Prior Auth request for Patient {PatientId}, CPT {Cpt}",
            command.PatientId, command.ProcedureCode);

        var paRepo = _unitOfWork.Repository<PriorAuthorization>();

        var pa = new PriorAuthorization
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = command.ClinicalNoteId,
            PatientId = command.PatientId,
            InsuranceProvider = command.InsuranceProvider,
            MemberId = command.MemberId,
            ProcedureCode = command.ProcedureCode,
            DiagnosisCode = command.DiagnosisCode,
            ClinicalJustification = command.ClinicalJustification,
            Status = PriorAuthStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        // Simulated instant rule engine: auto-approve standard minor procedures, auto-assign auth number
        if (command.ProcedureCode.StartsWith("99")) // E/M codes usually don't need PA
        {
            pa.Approve($"PA-AUTO-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                DateTime.UtcNow, DateTime.UtcNow.AddDays(90));
        }
        else
        {
            // Set pending review
            pa.Status = PriorAuthStatus.PendingClinicalReview;
        }

        await paRepo.AddAsync(pa, cancellationToken);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = pa.Id,
            EventType = "PriorAuthorizationRequestedEvent",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                PriorAuthId = pa.Id,
                pa.PatientId,
                pa.ProcedureCode,
                Status = pa.Status.ToString()
            }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Prior Auth {PaId} created with status {Status}", pa.Id, pa.Status);
    }
}


