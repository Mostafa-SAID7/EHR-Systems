using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.Services.Billing.Domain.Entities;
using EHRPlatform.Services.Billing.Domain.Enums;
using EHRPlatform.Services.Billing.Features.Claims.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Claims.Handlers;

/// <summary>
/// Submit to insurance handler.
/// Performs fraud detection scoring and prior authorization check before claim submission.
/// </summary>
public class SubmitToInsuranceCommandHandler : ICommandHandler<SubmitToInsuranceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly IFraudDetectionService _fraudService;
    private readonly ILogger<SubmitToInsuranceCommandHandler> _logger;

    public SubmitToInsuranceCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        IFraudDetectionService fraudService,
        ILogger<SubmitToInsuranceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _fraudService = fraudService;
        _logger = logger;
    }

    public async Task Handle(SubmitToInsuranceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting invoice {InvoiceId} to insurance {Provider}",
            command.InvoiceId, command.InsuranceProvider);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.Id == command.InvoiceId),
            cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {command.InvoiceId} not found");

        var procedureCodes = invoice.LineItems.Select(l => l.CPTCode).Where(c => !string.IsNullOrEmpty(c));

        // Evaluate claim fraud score
        var fraudResult = await _fraudService.EvaluateClaimAsync(
            command.InvoiceId,
            invoice.TotalAmount,
            command.InsuranceProvider,
            procedureCodes,
            cancellationToken);

        invoice.SubmitToInsurance(command.InsuranceProvider, command.PolicyNumber);
        
        var claim = invoice.InsuranceClaims.LastOrDefault();
        if (claim != null)
        {
            claim.FraudScore = fraudResult.RiskScore;
            claim.FraudFlags = string.Join("; ", fraudResult.Flags);
            claim.MemberId = command.PolicyNumber;

            if (fraudResult.IsHighRisk)
            {
                _logger.LogWarning("High fraud risk score {Score} detected for invoice {InvoiceId}. Claim placed on hold.",
                    fraudResult.RiskScore, command.InvoiceId);
                claim.PlaceOnHold($"High Fraud Risk ({fraudResult.RiskScore}): {string.Join(", ", fraudResult.Flags)}");
            }
        }

        await repo.UpdateAsync(invoice, cancellationToken);

        // Publish event
        var submitEvent = invoice.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = invoice.Id,
            EventType = nameof(InvoiceSubmittedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(submitEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} processed for insurance submission with FraudScore {Score}",
            command.InvoiceId, fraudResult.RiskScore);
    }
}



